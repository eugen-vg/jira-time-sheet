namespace JiraTimeSheet;

public interface IDeviceCodeService
{
    void StoreDeviceCode(string code);
    string RetrieveDeviceCode();
}
