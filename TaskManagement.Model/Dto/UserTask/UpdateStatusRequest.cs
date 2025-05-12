using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Model.Dto.UserTask;

public class UpdateStatusRequest
{
    public int TaskId { get; set; }
    public int StatusId { get; set; }
}
