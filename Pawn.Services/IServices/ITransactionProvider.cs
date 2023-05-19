using System;
namespace Pawn.Services
{
    public interface ITransactionProvider : IDisposable
    {
        void Begin(System.Data.IsolationLevel isolationLevel);
        void Commit();
        void RollBack();
    }
}
