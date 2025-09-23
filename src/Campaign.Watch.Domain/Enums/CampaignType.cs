namespace Campaign.Watch.Domain.Enums
{
    /// <summary>
    /// Define o tipo de execução de uma campanha.
    /// </summary>
    public enum CampaignType
    {
        /// <summary>
        /// A campanha é executada apenas uma vez.
        /// </summary>
        Single = 1,        
       
        /// <summary>
        /// A campanha é executada repetidamente, com base em um agendamento.
        /// </summary>
        Recurrent = 2
    }
}