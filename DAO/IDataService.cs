using TruckScalesWeb.Models;

namespace TruckScalesWeb.DAO
{
    public interface IDataService
    {
        #region UserAndRoles
        void SetUser(User user);
        User GetUser();
        Task<List<Role>> GetAllRoles();
        Task<List<User>> GetAllUsers();
        Task<User> GetUser(string account);
        Task UpdateLoginTime(Guid id);
        Task AddUser(string account, string name, string passwordHash, bool isActive, List<Role> roles);
        Task ChangeUser(Guid Id, string account, string name, string passwordHash, bool isActive, List<Role> roles, bool updatePassword);
        #endregion
        #region Cars
        Task<List<Country>> GetAllCountries();
        Task<Country> AddCountry(string fullName, string shortName);
        Task<Country> UpdateCountry(int id, string fullName, string shortName);
        Task<bool> MoveCountry(int position, int direction);
        Task<List<CarType>> GetAllCarTypes();
        Task<CarType> AddCarType(string name, bool canBePrimary, bool canBeSecondary);
        Task<CarType> UpdateCarType(int id, string name, bool canBePrimary, bool canBeSecondary);
        Task<bool> MoveCarType(int position, int direction);
        Task<List<Car>> GetAllCars();
        Task<Car> AddCar(string gosNumber, string model, int countryId, int carTypeId, bool isActive);
        Task<Car> UpdateCar(Guid id, string gosNumber, string model, int countryId, int carTypeId, bool isActive);
        #endregion
        #region Materials
        Task<List<Material>> GetAllMaterials();
        Task<Material> AddMatarial(string name, bool canBeSingle, bool canBeTransit);
        Task<Material> UpdateMaterial(Guid id, string name, bool canBeSingle, bool canBeTransit, bool isActive);
        Task<bool> MoveMaterial(int position, int direction);
        #endregion
        #region Weighings
        Task<string> GetNextTalonNumber();
        void SetPhotoUrls(List<string> urls);
        List<string> GetPhotoUrls();
        Task<Weighing> CreateNewWeighing(string talon, bool isEmpty, int weight, Guid operatorId, bool isManual, List<Photo> photos, WeightTypes weightType, List<Guid> selectedCarIDs, Guid? materialId);
        Task<Weighing> CloseStandardWeighing(Guid weighingId, int weight, Guid operatorId, bool isManual, List<Photo> photos, Guid materialId);
        Task<List<Weighing>> GetNotClosedWeighings();
        Task<List<Weighing>> GetAllWeighings(DateTime startDate, DateTime endDate);
        #endregion
    }
}
