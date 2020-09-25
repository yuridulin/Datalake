namespace iNOPC.Server.Web.RequestTypes
{
    public class DeviceForm
    {
        public int Id { get; set; } = 0;

        public string Name { get; set; }

        public bool AutoStart { get; set; }

        public string Configuration { get; set; }
    }
}