using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Campaign.Watch.Application.Helpers
{
    public static class SchedulerHelper
    {
        /// <summary>
        /// Obtém a próxima data de execução baseada na expressão crontab.
        /// </summary>
        /// <param name="crontabExpression">Expressão crontab (normalmente 5 campos: min hour day month dayweek).</param>
        /// <param name="fromDate">Data base para calcular (opcional, usa DateTime.UtcNow se não informado).</param>
        /// <returns>A próxima data de execução ou null se a expressão for inválida.</returns>
        public static DateTime? GetNextExecution(string crontabExpression, DateTime? fromDate = null)
        {
            if (string.IsNullOrWhiteSpace(crontabExpression))
            {
                return null;
            }

            try
            {
                var cleanExpression = NormalizeCrontabExpression(crontabExpression);
                var schedule = CrontabSchedule.Parse(cleanExpression);
                var baseDate = fromDate ?? DateTime.UtcNow;
                return schedule.GetNextOccurrence(baseDate);
            }
            catch (CrontabException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao processar crontab '{crontabExpression}': {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro inesperado ao processar crontab '{crontabExpression}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtém múltiplas próximas execuções de uma expressão crontab.
        /// </summary>
        /// <param name="crontabExpression">A expressão crontab.</param>
        /// <param name="count">O número de ocorrências a serem retornadas.</param>
        /// <param name="fromDate">A data a partir da qual o cálculo deve começar.</param>
        /// <returns>Uma lista enumerável com as próximas datas de execução.</returns>
        public static IEnumerable<DateTime> GetNextExecutions(string crontabExpression, int count = 5, DateTime? fromDate = null)
        {
            if (string.IsNullOrWhiteSpace(crontabExpression) || count <= 0)
            {
                return Enumerable.Empty<DateTime>();
            }

            CrontabSchedule schedule;
            try
            {
                var cleanExpression = NormalizeCrontabExpression(crontabExpression);
                schedule = CrontabSchedule.Parse(cleanExpression);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao processar múltiplas execuções para '{crontabExpression}': {ex.Message}");
                return Enumerable.Empty<DateTime>();
            }

            // A lógica de iteração (yield) é movida para um método auxiliar fora do try-catch.
            return GetNextOccurrencesIterator(schedule, count, fromDate);
        }

        /// <summary>
        /// Método auxiliar privado que realiza a iteração para GetNextExecutions.
        /// </summary>
        private static IEnumerable<DateTime> GetNextOccurrencesIterator(CrontabSchedule schedule, int count, DateTime? fromDate)
        {
            var current = fromDate ?? DateTime.UtcNow;
            for (int i = 0; i < count; i++)
            {
                current = schedule.GetNextOccurrence(current);
                yield return current;
            }
        }

        /// <summary>
        /// Verifica se uma expressão crontab é válida.
        /// </summary>
        /// <param name="crontabExpression">A expressão a ser validada.</param>
        /// <returns>True se a expressão for válida, caso contrário, false.</returns>
        public static bool IsValidCrontabExpression(string crontabExpression)
        {
            if (string.IsNullOrWhiteSpace(crontabExpression))
            {
                return false;
            }

            try
            {
                var cleanExpression = NormalizeCrontabExpression(crontabExpression);
                CrontabSchedule.Parse(cleanExpression);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica se uma campanha deveria ter sido executada baseada no seu cronograma.
        /// </summary>
        /// <param name="crontabExpression">A expressão crontab da campanha.</param>
        /// <param name="startDateTime">A data/hora de início configurada para a campanha.</param>
        /// <param name="lastExecution">A data da última execução conhecida (opcional).</param>
        /// <param name="checkTime">A data/hora atual da verificação (opcional, usa UtcNow como padrão).</param>
        /// <returns>True se a execução estiver atrasada, caso contrário, false.</returns>
        public static bool ShouldHaveExecuted(
            string crontabExpression,
            DateTime startDateTime,
            DateTime? lastExecution = null,
            DateTime? checkTime = null)
        {
            var now = checkTime ?? DateTime.UtcNow;

            if (now < startDateTime)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(crontabExpression))
            {
                return now > startDateTime && lastExecution == null;
            }

            try
            {
                var schedule = CrontabSchedule.Parse(NormalizeCrontabExpression(crontabExpression));
                // Define a data base para o cálculo: a última execução ou a data de início da campanha.
                var baseDate = lastExecution ?? startDateTime.AddMinutes(-1);

                var expectedExecution = schedule.GetNextOccurrence(baseDate);

                return now > expectedExecution;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Normaliza a expressão crontab para o formato de 5 campos esperado pela NCrontab.
        /// Remove o campo de segundos (início) ou de ano (fim), se existirem.
        /// </summary>
        private static string NormalizeCrontabExpression(string crontabExpression)
        {
            var parts = crontabExpression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 6)
            {
                // Se o último campo for um ano (comum em agendadores como Quartz.NET)
                if (int.TryParse(parts.Last(), out var year) && year > 1970)
                {
                    return string.Join(" ", parts.Take(5));
                }
                // Se o primeiro campo for para segundos
                else
                {
                    return string.Join(" ", parts.Skip(1));
                }
            }
            return crontabExpression;
        }
    }
}