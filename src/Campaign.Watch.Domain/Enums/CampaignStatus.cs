namespace Campaign.Watch.Domain.Enums
{
    /// <summary>
    /// Define os possíveis status de uma campanha, refletindo seu estado no sistema de origem.
    /// </summary>
    public enum CampaignStatus
    {
        /// <summary>
        /// A campanha está em modo de rascunho e ainda não foi ativada.
        /// </summary>
        Draft = 0,

        /// <summary>
        /// A campanha foi concluída com sucesso.
        /// </summary>
        Completed = 1,

        /// <summary>
        /// A campanha encontrou um erro e foi interrompida.
        /// </summary>
        Error = 3,

        /// <summary>
        /// A campanha está atualmente em execução.
        /// </summary>
        Executing = 5,

        /// <summary>
        /// A campanha está agendada para ser executada em uma data futura.
        /// </summary>
        Scheduled = 7,

        /// <summary>
        /// A campanha foi cancelada permanentemente.
        /// </summary>
        Canceled = 8,

        /// <summary>
        /// A campanha está no processo de ser cancelada.
        /// </summary>
        Canceling = 9
    }
}