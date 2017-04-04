using R3E.Data;

namespace R3E
{
    public interface R3EView
    {
        void ShowConnectionError(string message);
        void ShowData(Shared shared);
        void ShowMappingError(string message);
        void ShowRreRunning(bool e);
        void ShowSending(bool v);
    }
}