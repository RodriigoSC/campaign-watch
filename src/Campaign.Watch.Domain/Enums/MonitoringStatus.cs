namespace Campaign.Watch.Domain.Enums
{
    public enum MonitoringStatus
    {
        /// <summary>
        /// A campanha está agendada mas ainda não teve sua primeira execução.
        /// </summary>
        Pending,

        /// <summary>
        /// A campanha está em andamento. O worker está ativamente monitorando seus steps.
        /// Válido para campanhas pontuais com pausas internas.
        /// </summary>
        InProgress,

        /// <summary>
        /// A campanha concluiu sua última execução com sucesso e agora aguarda o próximo ciclo.
        /// (Específico para campanhas recorrentes).
        /// </summary>
        WaitingForNextExecution,

        /// <summary>
        /// A campanha (sendo pontual) foi concluída com sucesso e não será mais monitorada.
        /// </summary>
        Completed,

        /// <summary>
        /// Ocorreu um erro na última tentativa de execução que precisa de atenção.
        /// </summary>
        Failed,

        /// <summary>
        /// A campanha já deveria ter iniciado sua execução, mas não há registros de que isso aconteceu.
        /// </summary>
        ExecutionDelayed
    }
}
