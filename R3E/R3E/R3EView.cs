using R3E.Data;
using R3E.Model;

namespace R3E
{
    public interface R3EView
    {
        void ShowConnectionError(string message);
        void ShowSharedData(Shared shared);
        void ShowMappingError(string message);
        void ShowRreRunning(bool e);
        void ShowSending(bool v);
        void UpdateConfig(Config config);
        void onChangeTime(double e);
        void onEndTime(double e);
        void ShowDisplayData(DisplayData actualData);
    }
}