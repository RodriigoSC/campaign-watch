namespace Campaign.Watch.Domain.Enums
{
    /// <summary>
    /// Define o tipo de execução de uma campanha.
    /// </summary>
    public enum TypeCampaign
    {
        /// <summary>
        /// A campanha é executada repetidamente, com base em um agendamento.
        /// </summary>
        Recorrente = 1,

        /// <summary>
        /// A campanha é executada apenas uma vez.
        /// </summary>
        Pontual = 2
    }
}