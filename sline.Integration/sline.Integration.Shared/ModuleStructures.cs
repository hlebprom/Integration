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

  [Public]
  partial class ContractStr
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string DocumentKind { get; set; }
    public string Counterparty { get; set; }
    public string BusinessUnit { get; set; }
    public string Subject { get; set; }
    public string Note { get; set; }
    public string OurSignatory { get; set; }
    public string DocumentGroup { get; set; }
    public string Department { get; set; }
    public int? CounterpartySignatory { get; set; }
    public double? TotalAmount { get; set; }
    public int? Currency { get; set; }
    public int? Contact { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTill { get; set; }
    public string ResponsibleEmployee { get; set; }
    public string Assignee { get; set; }
    public bool? IsStandard { get; set; }
    public bool? IsAutomaticRenewal { get; set; }
    public string RegistrationNumber { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public string LastVersionFileName { get; set; }
    public string LastVersionBody { get; set; }
    public DateTime? Created { get; set; }
    public bool? APIUpdatedhprom { get; set; }
    public bool? ActionWebApi { get; set; }
    public string Status { get; set; }
  }

  [Public]
  partial class DocumentStr
  {
    public int Id { get; set; }                     // ИД Документа
    public string DocumentId { get; set; }          // ГУИД Документа
    public string DocumentKind { get; set; }        // Вид докумена - строка
    public string LeadingDocumentId { get; set; }   // ИД Заявления для приказа. Если не заполнен?
    public string DocumentDate { get; set; }
    public string DocumentNumber { get; set; }
    public string AuthorExtId { get; set; }         // Автор документа - сотрудник
    public string BusinessUnit { get; set; }        // Организация автора
    public string Department { get; set; }          // Отдел автора приказа
    public string Signatory { get; set; }
    public string EmployeeExtId { get; set; }
    public string DocExt { get; set; }              // Расширение файла документа
    public string LastVersionBody { get; set; }     // Base64 string
    public string ApprovalRule { get; set; }
    public bool Trace { get; set; }
  }
  
  [Public]
  partial class EmployeeStr
  {
    public int Id { get; set; }
    public string ExtId { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string TIN { get; set; }
    public string INILA { get; set; }
    public string DateOfBirth { get; set; }
    public string Sex { get; set; }
    public Guid? Sid { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool? IsSystem { get; set; }
    public string Login { get; set; }
    public string Person { get; set; }
    public string Department { get; set; }
    public string JobTitle { get; set; }
    public string Phone { get; set; }
    public string Note { get; set; }
    public string Email { get; set; }
    public string AdministrativeManager { get; set; }
    public string FunctionalManager { get; set; }
    public bool? NeedNotifyExpiredAssignments { get; set; }
    public bool? NeedNotifyNewAssignments { get; set; }
    public string Status { get; set; }
    public string EmployeeNumber { get; set; }
  }

}