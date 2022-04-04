namespace Project2FA.Repository.Database
{
    public interface IProject2FARepository
    {
        IDatafileRepository Datafile { get; }

        IPasswordHashRepository Password { get; }
    }
}
