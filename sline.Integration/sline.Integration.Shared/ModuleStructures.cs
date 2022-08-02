using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace sline.Integration.Structures.Module
{

  [Public]
  partial class LoginStr
  {
    public int Id { get; set; }
    public string LoginName { get; set; }
    public string TypeAuthentication { get; set; }
    public bool? NeedChangePassword { get; set; }
    public string Status { get; set; }
  }
  
  [Public]
  partial class DepartmentStr
  {
    public int Id { get; set; }
    public string ExtId { get; set; }
    public Guid? Sid { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool? IsSystem { get; set; }
    public string Manager { get; set; }
    public string HeadOffice { get; set; }
    public string Phone { get; set; }
    public string ShortName { get; set; }
    public string Note { get; set; }
    public string BusinessUnit { get; set; }
    public string Code { get; set; }
    public string Status { get; set; }
  }

  [Public]
  partial class ContactStr
  {
    public int Id { get; set; }
    public string ExtId { get; set; }
    public string Name { get; set; }
    public string Person { get; set; }
    public string Company { get; set; }
    public string Department { get; set; }
    public string JobTitle { get; set; }
    public string Phone { get; set; }
    public string Fax { get; set; }
    public string Email { get; set; }
    public string Note { get; set; }
    public string Homepage { get; set; }
    public string Status { get; set; }
  }
  
  [Public]
  partial class CompanyStr
  {
    //counterparty
    public int Id { get; set; }
    public string ExtId { get; set; }
    public string Name { get; set; }
    public string TIN { get; set; }
    public int? CityId { get; set; }
    public int? RegionId { get; set; }
    public string LegalAddress { get; set; }
    public string PostalAddress { get; set; }
    public string Phones { get; set; }
    public string Email { get; set; }
    public string Homepage { get; set; }
    public string Note { get; set; }
    public bool? Nonresident { get; set; }
    public string PSRN { get; set; }
    public string NCEO { get; set; }
    public string NCEA { get; set; }
    public string Account { get; set; }
    public int? BankId { get; set; }
    public bool? CanExchange { get; set; }
    public string Status { get; set; }
    //companybase
    public string TRRC { get; set; }
    public string LegalName { get; set; }
    public int? HeadCompanyId { get; set; }
    public bool? IsCardReadOnly { get; set; }
    //company
    public string Responsible { get; set; }
    public string Code { get; set; }
    public string City { get; set; }
    public string Region { get; set; }
    public string Bank { get; set; }
    public string HeadCompany { get; set; }
  }
  
  [Public]
  partial class CityStr
  {
    public int Id { get; set; }
    public string ExtId { get; set; }
    public string Name { get; set; }
    public string Region { get; set; }
    public string Country { get; set; }
    public string Status { get; set; }
  }
  
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
  partial class DocumentOrderStr
  {
    public int Id { get; set; }
    public string DocumentId { get; set; }
    public string DocumentKind { get; set; }
    public string LeadingDocumentId { get; set; }
    public string DocumentDate { get; set; }
    public string DocumentNumber { get; set; }
    public string AuthorExtId { get; set; }
    public string BusinessUnit { get; set; }
    public string Department { get; set; }
    public string Signatory { get; set; }
    public string EmployeeExtId { get; set; }
    public string DocExt { get; set; }
    public string LastVersionBody { get; set; }
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
    public string LegalAddress { get; set; }
    public string PostalAddress { get; set; }
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

  [Public]
  partial class SupAgreementStr
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string DocumentKind { get; set; }
    public int? LeadingDocument { get; set; }
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
    public bool? IsIsAutomaticRenewal { get; set; }
    public string RegistrationNumber { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public string LastVersionFileName { get; set; }
    public string LastVersionBody { get; set; }
    public string Counterparty { get; set; }
    public DateTime? Created { get; set; }
    public bool? APIUpdatedhprom { get; set; }
    public bool? ActionWebApi { get; set; }
    public string Status { get; set; }
  }

  [Public]
  partial class JobTitleStr
  {
    public int Id { get; set; }
    public string ExtId { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
  }
  
  
  
  
}