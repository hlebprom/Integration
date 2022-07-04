using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace sline.Integration.Structures.Module
{
  [Public]
  partial class AcquaintanceTaskStr
  {
    public int Id { get; set; }
    public string Subject { get; set; }
    public string Author { get; set; }
    public string Performer { get; set; }
    public string Status { get; set; }
    public string Importance { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? Created { get; set; }
    public string ExtId { get; set; }
    public string EmployeeExtId { get; set; }
    public int DocumentId { get; set; }
    public int MainTask { get; set; }
  }

}