using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleClient.Models
{
    public class ProducerModel

    {
    public int Id { get; set; }
    public string Description { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsDisabled { get; set; }
    public bool IsProducerDown { get; set; }
    }
}
