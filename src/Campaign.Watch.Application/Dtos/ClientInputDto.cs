using Campaign.Watch.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Dtos
{
    public class ClientInputDto
    {
        [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
        public string Name { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public CampaignConfigInputDto CampaignConfig { get; set; }

        public List<EffectiveChannelInputDto> EffectiveChannels { get; set; }
    }

    public class CampaignConfigInputDto
    {
        [Required]
        public string ProjectID { get; set; }
        public string Database { get; set; }
    }

    public class EffectiveChannelInputDto
    {
        public TypeChannels TypeChannel { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Database { get; set; }
        public string TenantID { get; set; }
    }
}
