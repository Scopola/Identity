namespace ForcedLogInApp.Helpers
{
    public enum IdentityAuthType
    {
        // Common: AAD and MSA
        Common = 1,
        // Consumers: MSA
        Consumers = 2,
        // Organizations: All AAD and Integrated Auth
        Organizations = 3,
        // Tenant: Single domain AAD and Integrated Auth
        Tenant = 4
    }
}
