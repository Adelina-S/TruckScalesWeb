using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TruckScalesWeb.Models;

namespace TruckScalesWeb.DAO
{
    public class DataService : IDataService
    {
        private readonly IDbContextFactory<SqlContext> _contextFactory;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);
        private User User;
        private List<string> PhotoUrls;
        public DataService(IDbContextFactory<SqlContext> contextFactory, IMemoryCache cache)
        {
            _contextFactory = contextFactory;
            _cache = cache;
        }
        #region UserAndRoles
        public void SetUser(User user) => User = user;
        public User GetUser() => User;
        public async Task<List<Role>> GetAllRoles()
        {
            if (_cache.TryGetValue("Roles", out List<Role>  roles)) return roles;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var bdRoles = await context.Roles.AsNoTracking().ToListAsync();
            _cache.Set("Roles", bdRoles, _cacheExpiration);
            return bdRoles;
        }
        public async Task<List<User>> GetAllUsers()
        {
            if (_cache.TryGetValue("Users", out List<User> users)) return users;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var bdUsers = await context.Users.Include(t => t.Roles).AsNoTracking().ToListAsync();
            _cache.Set("Users", bdUsers, _cacheExpiration);
            return bdUsers;
        }
        public async Task<User> GetUser(string account)
        {
            if (_cache.TryGetValue("Users", out List<User> users))
                return users.FirstOrDefault(t => t.Account == account);
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users.Include(t => t.Roles).AsNoTracking().FirstOrDefaultAsync(t => t.Account == account);
        }
        public async Task UpdateLoginTime(Guid id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var user = context.Users.FirstOrDefault(t => t.Id == id);
            if (user != null)
            {
                user.LastLogin = DateTime.UtcNow;
                await context.SaveChangesAsync();
                _cache.Remove("Users");
            }
        }
        public async Task AddUser(string account, string name, string passwordHash, bool isActive, List<Role> roles)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            User user = new User
            {
                Id=Guid.NewGuid(),
                Account = account,
                DisplayName = name,
                PasswordHash = passwordHash,
                IsActive = isActive,
                LastLogin = DateTime.MinValue
            };
            if (roles.Any() == true)
            {
                var bdRoles = await context.Roles.Where(t => roles.Select(t => t.Id).Contains(t.Id)).ToListAsync();
                foreach (var role in bdRoles) user.Roles.Add(role);
            }
            context.Users.Add(user);
            await context.SaveChangesAsync();
            _cache.Remove("Users");
        }
        public async Task ChangeUser(Guid Id, string account, string name, string passwordHash, bool isActive, List<Role> roles, bool updatePassword)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var existingUser = await context.Users.Include(t => t.Roles).FirstOrDefaultAsync(t => t.Id == Id);
            if (existingUser == null) return;
            existingUser.Account = account;
            existingUser.DisplayName = name;
            if (updatePassword)
                existingUser.PasswordHash = passwordHash;
            existingUser.IsActive = isActive;
            var bdRoles = await context.Roles.Where(t => roles.Select(t => t.Id).Contains(t.Id)).ToListAsync();
            existingUser.Roles.Clear();
            foreach (var role in bdRoles) existingUser.Roles.Add(role);

            await context.SaveChangesAsync();
            _cache.Remove("Users");
        }
        #endregion
        #region Cars
        #region Countries
        public async Task<List<Country>> GetAllCountries()
        {
            if (_cache.TryGetValue("Countries", out List<Country> countries)) return countries;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var bdCountries = await context.Countries.OrderBy(t=>t.Sorter).AsNoTracking().ToListAsync();
            _cache.Set("Countries", bdCountries, _cacheExpiration);
            return bdCountries;
        }
        public async Task<Country> AddCountry(string fullName, string shortName)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var lastSorterCountry = await context.Countries.OrderByDescending(t => t.Sorter).FirstOrDefaultAsync();
            var sorter = (lastSorterCountry?.Sorter ?? -1) + 1;
            var country = new Country
            {
                FullName = fullName,
                ShortName = shortName,
                Sorter = sorter
            };
            context.Countries.Add(country);
            await context.SaveChangesAsync();
            _cache.Remove("Countries");
            _cache.Remove("Cars");
            return country;
        }
        public async Task<Country> UpdateCountry(int id, string fullName, string shortName)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var country = await context.Countries.FirstOrDefaultAsync(t => t.Id == id);
            if (country == null) return null;
            country.FullName = fullName;
            country.ShortName = shortName;
            await context.SaveChangesAsync();
            _cache.Remove("Countries");
            _cache.Remove("Cars");
            return country;
        }
        public async Task<bool> MoveCountry(int position, int direction)
        {
            if (position == direction) return false;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var countries = await context.Countries.Where(t => t.Sorter == position || t.Sorter == direction).ToListAsync();
            if (countries.Count<2) return false;
            var temp = countries.Where(t => t.Sorter == position).ToList();
            foreach (var c in countries.Where(t => t.Sorter == direction))
                c.Sorter = position;
            foreach (var c in temp)
                c.Sorter = direction;
            await context.SaveChangesAsync();
            _cache.Remove("Countries");
            return true;
        }
        #endregion
        #region CarTypes
        public async Task<List<CarType>> GetAllCarTypes()
        {
            if (_cache.TryGetValue("CarTypes", out List<CarType> carTypes)) return carTypes;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var bdCarTypes = await context.CarTypes.OrderBy(t => t.Sorter).AsNoTracking().ToListAsync();
            _cache.Set("CarTypes", bdCarTypes, _cacheExpiration);
            return bdCarTypes;
        }

        public async Task<CarType> AddCarType(string name, bool canBePrimary, bool canBeSecondary)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var lastSorterCarType = await context.CarTypes.OrderByDescending(t => t.Sorter).FirstOrDefaultAsync();
            var sorter = (lastSorterCarType?.Sorter ?? -1) + 1;
            var carType = new CarType
            {
                Name = name,
                CanBePrimary = canBePrimary,
                CanBeSecondary = canBeSecondary,
                Sorter = sorter
            };
            context.CarTypes.Add(carType);
            await context.SaveChangesAsync();
            _cache.Remove("CarTypes");
            _cache.Remove("Cars");
            return carType;
        }

        public async Task<CarType> UpdateCarType(int id, string name, bool canBePrimary, bool canBeSecondary)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var carType = await context.CarTypes.FirstOrDefaultAsync(t => t.Id == id);
            if (carType == null) return null;
            carType.Name = name;
            carType.CanBePrimary = canBePrimary;
            carType.CanBeSecondary=canBeSecondary;
            await context.SaveChangesAsync();
            _cache.Remove("CarTypes");
            _cache.Remove("Cars");
            return carType;
        }

        public async Task<bool> MoveCarType(int position, int direction)
        {
            if (position == direction) return false;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var carTypes = await context.CarTypes.Where(t => t.Sorter == position || t.Sorter == direction).ToListAsync();
            if (carTypes.Count < 2) return false;
            var temp = carTypes.Where(t => t.Sorter == position).ToList();
            foreach (var c in carTypes.Where(t => t.Sorter == direction))
                c.Sorter = position;
            foreach (var c in temp)
                c.Sorter = direction;
            await context.SaveChangesAsync();
            _cache.Remove("CarTypes");
            return true;
        }
        #endregion
        #region Cars
        public async Task<List<Car>> GetAllCars()
        {
            if (_cache.TryGetValue("Cars", out List<Car> cars)) return cars;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var bdCars = await context.Cars.Include(t=>t.Country).Include(t=>t.CarType).OrderByDescending(t => t.LastUsedTime).AsNoTracking().ToListAsync();
            _cache.Set("Cars", bdCars, _cacheExpiration);
            return bdCars;
        }

        public async Task<Car> AddCar(string gosNumber, string model, int countryId, int carTypeId, bool isActive)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var country = await context.Countries.FirstOrDefaultAsync(t => t.Id == countryId);
            if (country is null) return null;
            var carType = await context.CarTypes.FirstOrDefaultAsync(t => t.Id == carTypeId);
            if (carType is null) return null;
            
            var car = new Car
            {
                GosNumber = gosNumber,
                Model = model,
                Country = country,
                CarType = carType,
                IsActive = isActive,
                LastUsedTime = null
            };
            context.Cars.Add(car);
            await context.SaveChangesAsync();
            _cache.Remove("Cars");
            return car;
        }

        public async Task<Car> UpdateCar(Guid id, string gosNumber, string model, int countryId, int carTypeId, bool isActive)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var car = await context.Cars.FirstOrDefaultAsync(t => t.Id == id);
            var country = await context.Countries.FirstOrDefaultAsync(t => t.Id == countryId);
            if (country is null) return null;
            var carType = await context.CarTypes.FirstOrDefaultAsync(t => t.Id == carTypeId);
            if (carType is null) return null;
            car.GosNumber = gosNumber;
            car.Model = model;
            car.Country = country;
            car.CarType = carType;
            car.IsActive = isActive;
            await context.SaveChangesAsync();
            _cache.Remove("Cars");
            return car;
        }
        #endregion
        #endregion
        #region Materials
        public async Task<List<Material>> GetAllMaterials()
        {
            if (_cache.TryGetValue("Materials", out List<Material> materials)) return materials;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var bdMaterials = await context.Materials.OrderBy(t => t.Sorter).AsNoTracking().ToListAsync();
            _cache.Set("Materials", bdMaterials, _cacheExpiration);
            return bdMaterials;
        }
        public async Task<Material> AddMatarial(string name, bool canBeSingle, bool canBeTransit)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var lastSorterMatarial = await context.Materials.OrderByDescending(t => t.Sorter).FirstOrDefaultAsync();
            var sorter = (lastSorterMatarial?.Sorter ?? -1) + 1;
            var material = new Material
            {
                Id=Guid.NewGuid(),
                Name = name,
                CanBeSingle = canBeSingle,
                CanBeTransit=canBeTransit,
                IsActive=true,
                Sorter = sorter
            };
            context.Materials.Add(material);
            await context.SaveChangesAsync();
            _cache.Remove("Materials");
            return material;
        }
        public async Task<Material> UpdateMaterial(Guid id, string name, bool canBeSingle, bool canBeTransit, bool isActive)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var material = await context.Materials.FirstOrDefaultAsync(t => t.Id == id);
            if (material == null) return null;
            material.Name = name;
            material.CanBeSingle = canBeSingle;
            material.CanBeTransit = canBeTransit;
            material.IsActive = isActive;
            await context.SaveChangesAsync();
            _cache.Remove("Materials");
            return material;
        }
        public async Task<bool> MoveMaterial(int position, int direction)
        {
            if (position == direction) return false;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var materials = await context.Materials.Where(t => t.Sorter == position || t.Sorter == direction).ToListAsync();
            if (materials.Count < 2) return false;
            var temp = materials.Where(t => t.Sorter == position).ToList();
            foreach (var c in materials.Where(t => t.Sorter == direction))
                c.Sorter = position;
            foreach (var c in temp)
                c.Sorter = direction;
            await context.SaveChangesAsync();
            _cache.Remove("Materials");
            return true;
        }
        #endregion
        #region Weighings
        public async Task<string> GetNextTalonNumber()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            // Получаем следующий номер талона для текущего года
            var currentYear = DateTime.Now.Year.ToString();
            var maxTalon = context.Weighings.Where(w => w.Talon.StartsWith(currentYear)).Select(w => w.Talon).Max();
            int nextTalon = 1;
            if (maxTalon is not null) nextTalon = Int32.Parse(maxTalon.Substring(4)) + 1;
            return $"{currentYear}{nextTalon.ToString("00000")}";
        }
        public void SetPhotoUrls(List<string> urls) => PhotoUrls = urls;
        public List<string> GetPhotoUrls() => PhotoUrls;
        public async Task<Weighing> CreateNewWeighing(string talon, bool isEmpty, int weight, Guid operatorId, bool isManual, List<Photo> photos, WeightTypes weightType, List<Guid> selectedCarIDs, Guid? materialId)
        {
            OneWeighing oneWeighing = new OneWeighing()
            {
                Id = Guid.NewGuid(),
                IsEmpty= isEmpty,
                Weight = weight,
                WeightTime=DateTime.Now,
                OperatorId=operatorId,
                IsManual=isManual,
                Photos=photos
            };
            foreach (var photo in photos) photo.OneWeighingId=oneWeighing.Id;
            Weighing weighing = new Weighing()
            {
                Id = Guid.NewGuid(),
                Talon= talon,
                OneWeighings = new List<OneWeighing> { oneWeighing },
                WeightType = weightType,
                MaterialId=materialId,
                OperatorId=operatorId,
            };
            oneWeighing.WeighingId = weighing.Id;
            if (weightType == WeightTypes.Single)
            {
                weighing.Brutto = weight;
                weighing.Netto = weight;
                weighing.CloseDate = oneWeighing.WeightTime;
            }
            else
            {
                if (isEmpty)
                    weighing.Tare = weight;
                else
                    weighing.Brutto = weight;
            }
            await using var context = await _contextFactory.CreateDbContextAsync();
            // Получаем следующий номер талона для текущего года
            var cars = (await context.Cars.Where(t => selectedCarIDs.Contains(t.Id)).ToListAsync()).OrderBy(c => selectedCarIDs.IndexOf(c.Id)).ToList();
            weighing.Cars.AddRange(cars);
            foreach (var car in cars) car.LastUsedTime = oneWeighing.WeightTime;
            context.Weighings.Add(weighing);
            await context.SaveChangesAsync();
            return weighing;
        }
        public async Task<Weighing> CloseStandardWeighing(Guid weighingId, int weight, Guid operatorId, bool isManual, List<Photo> photos, Guid materialId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var weighing = await context.Weighings.FirstOrDefaultAsync(t => t.Id == weighingId);
            if (weighing is null) return null;
            OneWeighing first = weighing.OneWeighings.First();
            OneWeighing second = new OneWeighing()
            {
                Id = Guid.NewGuid(),
                IsEmpty = !first.IsEmpty,
                Weight = weight,
                WeightTime = DateTime.Now,
                OperatorId = operatorId,
                IsManual = isManual,
                WeighingId = weighing.Id
            };

            foreach (var photo in photos)
            {
                photo.OneWeighingId = second.Id;
                context.Photo.Add(photo);
            }
            context.OneWeighings.Add(second);
            weighing.MaterialId = materialId;
            
            int tareWeight, bruttoWeight, nettoWeight;
            tareWeight = first.IsEmpty ? first.Weight : second.Weight;
            bruttoWeight = first.IsEmpty ? second.Weight : first.Weight;
            nettoWeight = bruttoWeight - tareWeight;
            weighing.Tare = tareWeight;
            weighing.Brutto = bruttoWeight;
            weighing.Netto = nettoWeight;
            weighing.CloseDate = second.WeightTime;

            await context.SaveChangesAsync();
            return weighing;
        }
        public async Task<List<Weighing>> GetNotClosedWeighings()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var result = await context.Weighings.Where(t => t.CloseDate == null).OrderBy(t => t.OneWeighings[0].WeightTime).AsNoTracking().ToListAsync();
            return result;
        }
        public async Task<List<Weighing>> GetAllWeighings(DateTime startDate, DateTime endDate)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Weighings
                .Where(t => t.OneWeighings[0].WeightTime >= startDate)
                .Where(t =>
                            (t.CloseDate == null && t.OneWeighings[0].WeightTime <= endDate) ||
                            (t.CloseDate != null && t.CloseDate <= endDate)
                        )
                .AsNoTracking()
                .ToListAsync();
        }
        #endregion
    }
}
