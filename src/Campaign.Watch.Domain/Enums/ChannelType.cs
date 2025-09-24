namespace Campaign.Watch.Domain.Enums
{
    /// <summary>
    /// Define os tipos de canais de comunicação disponíveis.
    /// </summary>
    public enum ChannelType
    {
        /// <summary>
        /// Canal de E-mail.
        /// </summary>
        EffectiveMail = 1,

        /// <summary>
        /// Canal de SMS (Short Message Service).
        /// </summary>
        EffectiveSms = 2,

        /// <summary>
        /// Canal de  Push.
        /// </summary>
        EffectivePush = 3,

        /// <summary>
        /// Canal de WhatsApp.
        /// </summary>
        EffectiveWhatsApp = 5,

        /// <summary>
        /// Canal de API.
        /// </summary>       
        EffectiveApi = 6
    }
}