using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Net.Mail;
using System.Net;
using hprom.DocflowExt;
using System.IO;

namespace sline.Integration.Server
{
  public class ModuleFunctions
  {
    
    #region Получение данных
    
    [Public(WebApiRequestType = RequestType.Get)]
    public List<Structures.Module.IContractStr> GetContracts(string sysCode)
    {
      List<Structures.Module.IContractStr> result = new List<Structures.Module.IContractStr>();
      
      var documents = Contracts.GetAll();
      if (sysCode == "1C")
        documents = documents.Where(d => d.DocumentKind.ExportTo1Chprom == true && d.APIUpdatedhprom != false);
      if (sysCode == "AX")
        documents = documents.Where(d => d.DocumentKind.ExportToAXhprom == true && d.APIUpdatedhprom != false);

      foreach (var doc in documents)
      {
        if (!doc.DocumentKind.Name.ToLower().Contains("нетиповой"))
        {
          try
          {
            var docDto = Structures.Module.ContractStr.Create();
            docDto = CopyFromEntity(doc);
            docDto.APIUpdatedhprom = false;
            docDto.ActionWebApi = true;
            docDto = SaveEntity(docDto);
            
            result.Add(docDto);
          }
          catch (Exception exc)
          {
            try
            {
              var docDto = Structures.Module.ContractStr.Create();
              docDto.Id = doc.Id;
              docDto.Note = exc.Message;
              
              result.Add(docDto);
            }
            catch
            { }
          }
        }
        else
        {
          if (doc.InternalApprovalState == Sungero.Docflow.OfficialDocument.InternalApprovalState.Signed
              || doc.InternalApprovalState == Sungero.Docflow.OfficialDocument.InternalApprovalState.PendingSign)
          {
            try
            {
              var docDto = Structures.Module.ContractStr.Create();
              docDto = CopyFromEntity(doc);
              docDto.APIUpdatedhprom = false;
              docDto.ActionWebApi = true;
              docDto = SaveEntity(docDto);
              
              result.Add(docDto);
            }
            catch (Exception exc)
            {
              try
              {
                var docDto = Structures.Module.ContractStr.Create();
                docDto.Id = doc.Id;
                docDto.Note = exc.Message;
                
                result.Add(docDto);
              }
              catch
              { }
            }
          }
        }
      }

      return result;
    }
    public Structures.Module.IContractStr CopyFromEntity(IContract entity)
    {
      var docDto = Structures.Module.ContractStr.Create();
      docDto.Id = entity.Id;
      docDto.Name = entity.Name;
      docDto.DocumentKind = entity.DocumentKind?.Name;
      
      var bu = BusinessUnits.As(entity.BusinessUnit);
      docDto.BusinessUnit = bu?.ExtIdhprom;
      docDto.Subject = entity.Subject;
      docDto.Note = entity.Note;
      
      var ourSign = Employees.As(entity.OurSignatory);
      docDto.OurSignatory = ourSign?.ExtIdhprom;
      docDto.DocumentGroup = entity.DocumentGroup?.Name;
      
      var dep = Departments.As(entity.OurSignatory);
      docDto.Department = dep?.ExtIdhprom;
      docDto.IsAutomaticRenewal = entity.IsAutomaticRenewal;
      docDto.RegistrationNumber = entity.RegistrationNumber;
      docDto.RegistrationDate = entity.RegistrationDate;

      var counterparty = entity.Counterparty;
      if (Companies.Is(counterparty))
        docDto.Counterparty = Companies.As(counterparty)?.ExtIdhprom;

      docDto.Created = entity.Created;
      docDto.CounterpartySignatory = entity.CounterpartySignatory?.Id;
      if (entity.TotalAmount == null)
        docDto.TotalAmount = 0;
      else
        docDto.TotalAmount = entity.TotalAmount;
      
      docDto.Currency = entity.Currency?.Id;
      docDto.Contact = entity.Contact?.Id;
      docDto.ValidFrom = entity.ValidFrom;
      docDto.ValidTill = entity.ValidTill;

      var resp = Employees.As(entity.Responsiblehprom);
      docDto.ResponsibleEmployee = resp?.ExtIdhprom;
      docDto.IsStandard = entity.IsStandard;
      docDto.APIUpdatedhprom = entity.APIUpdatedhprom;
      docDto.ActionWebApi = entity.ActionWebApihprom;

      docDto.Status = GetContractStatus(docDto.Id);
      docDto.APIUpdatedhprom = false;
      docDto.ActionWebApi = true;
      
      return docDto;
    }
    public Structures.Module.IContractStr SaveEntity(Structures.Module.IContractStr docDto)
    {
      var entity = Contracts.GetAll().FirstOrDefault(x => Equals(x.Id, docDto.Id)) ?? Contracts.Create();
      entity.ActionWebApihprom = docDto.ActionWebApi;
      entity.Save();
      docDto = CopyFromEntity(entity);
      
      var docVersion = entity.LastVersion;
      if (docVersion != null)
      {
        try
        {
          docDto.LastVersionFileName = $"{entity.Id}.{entity.LastVersion.AssociatedApplication.Extension}";
          using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
          {
            if (docVersion.PublicBody.Size > 0)
            {
              var str = docVersion.PublicBody.Read();
              str.CopyTo(ms);
            }
            else if (docVersion.Body.Size > 0)
            {
              var str = docVersion.Body.Read();
              str.CopyTo(ms);
            }
            docDto.LastVersionBody = Convert.ToBase64String(ms.ToArray());
          }
        }
        catch (Exception exc)
        {
          Logger.Error($" >>> SOFTLINE >>> '{exc.Message}', {exc.StackTrace}");
        }
      }
      
      return docDto;
    }
    
    [Public(WebApiRequestType = RequestType.Get)]
    public List<Structures.Module.ISupAgreementStr> GetSupAgreements(string sysCode)
    {
      List<Structures.Module.ISupAgreementStr> result = new List<Structures.Module.ISupAgreementStr>();
      var documents = SupAgreements.GetAll();

      if (sysCode == "1C")
        documents = documents.Where(d => d.DocumentKind.ExportTo1Chprom == true && d.APIUpdatedhprom != false);
      if (sysCode == "AX")
        documents = documents.Where(d => d.DocumentKind.ExportToAXhprom == true && d.APIUpdatedhprom != false);

      foreach (var doc in documents)
      {
        if (doc.InternalApprovalState == Sungero.Docflow.OfficialDocument.InternalApprovalState.Signed
            || doc.InternalApprovalState == Sungero.Docflow.OfficialDocument.InternalApprovalState.PendingSign)
        {
          try
          {
            var docDto = Structures.Module.SupAgreementStr.Create();
            docDto = CopyFromEntity(doc);
            docDto.APIUpdatedhprom = false;
            docDto.ActionWebApi = true;
            docDto = SaveEntity(docDto);
            
            result.Add(docDto);
          }
          catch(Exception exc)
          {
            try
            {
              var docDto = Structures.Module.SupAgreementStr.Create();
              docDto.Id = doc.Id;
              docDto.Note = exc.Message;
              
              result.Add(docDto);
            }
            catch { }
          }
        }
      }

      return result;
    }
    public Structures.Module.ISupAgreementStr CopyFromEntity(ISupAgreement entity)
    {
      var docDto = Structures.Module.SupAgreementStr.Create();
      docDto.Id = entity.Id;
      docDto.Name = entity.Name;
      docDto.DocumentKind = entity.DocumentKind?.Name;

      docDto.LeadingDocument = entity.LeadingDocument?.Id;
      var bu = BusinessUnits.As(entity.BusinessUnit);
      docDto.BusinessUnit = bu?.ExtIdhprom;
      docDto.Subject = entity.Subject;
      docDto.Note = entity.Note;
      var ourSign = Employees.As(entity.OurSignatory);
      docDto.OurSignatory = ourSign?.ExtIdhprom;
      docDto.DocumentGroup = entity.DocumentGroup?.Name;
      // TODO RomanovSL Обсудить корректность реализации в WebApi
      /*var dep = entity.OurSignatory as hprom.DocflowExt.IDepartment;
      docDto.Department = dep?.ExtIdhprom;*/
      docDto.IsIsAutomaticRenewal = entity.LeadingDocument?.IsAutomaticRenewal;
      docDto.RegistrationNumber = entity.RegistrationNumber;
      docDto.RegistrationDate = entity.RegistrationDate;
      var counterparty = entity.Counterparty;
      if (Companies.Is(counterparty))
      {
        docDto.Counterparty = Companies.As(counterparty)?.ExtIdhprom;
      }
      docDto.CounterpartySignatory = entity.CounterpartySignatory?.Id;
      if (entity.TotalAmount == null)
        docDto.TotalAmount = 0;
      else
        docDto.TotalAmount = entity.TotalAmount;
      docDto.Currency = entity.Currency?.Id;
      docDto.Contact = entity.Contact?.Id;
      docDto.ValidFrom = entity.ValidFrom;
      docDto.ValidTill = entity.ValidTill;

      docDto.Created = entity.Created;

      var ldoc = Contracts.As(entity.LeadingDocument);
      var resp = Employees.As(ldoc?.Responsiblehprom);
      docDto.ResponsibleEmployee = resp?.ExtIdhprom;
      docDto.IsStandard = entity.IsStandard;
      docDto.APIUpdatedhprom = entity.APIUpdatedhprom;
      docDto.ActionWebApi = entity.ActionWebApihprom;
      docDto.Status = GetContractStatus(docDto.Id);
      
      return docDto;
    }
    public Structures.Module.ISupAgreementStr SaveEntity(Structures.Module.ISupAgreementStr docDto)
    {
      var entity = SupAgreements.GetAll().FirstOrDefault(x => Equals(x.Id, docDto.Id)) ?? SupAgreements.Create();
      entity.ActionWebApihprom = docDto.ActionWebApi;
      entity.Save();
      docDto = CopyFromEntity(entity);
      
      var docVersion = entity.LastVersion;
      if (docVersion != null)
      {
        try
        {
          docDto.LastVersionFileName = $"{entity.Id}.{entity.LastVersion.AssociatedApplication.Extension}";
          using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
          {
            if (docVersion.PublicBody.Size > 0)
            {
              var str = docVersion.PublicBody.Read();
              str.CopyTo(ms);
            }
            else if (docVersion.Body.Size > 0)
            {
              var str = docVersion.Body.Read();
              str.CopyTo(ms);
            }
            docDto.LastVersionBody = Convert.ToBase64String(ms.ToArray());
          }
        }
        catch (Exception exc)
        {
          Logger.Error($" >>> SOFTLINE >>> '{exc.Message}', {exc.StackTrace}");
        }
      }
      
      return docDto;
    }
    
    public string GetContractStatus(int idDoc)
    {
      var allTask = ApprovalTasks.GetAll().Where(x => x.DisplayValue.Contains($"ИД {idDoc}"));
      int id = 0;
      var mainTask = ApprovalTasks.Null;
      foreach (var item in allTask)
      {
        if (item.Id > id)
        {
          id = item.Id;
          mainTask = item;
        }
      }
      if (mainTask == null)
        return "Without task";
      else
      {
        if (mainTask.Status == Sungero.Workflow.Task.Status.Completed)
          return "Подписан";

        bool checkApprovalStatus = true;
        var allAssignments = Sungero.Workflow.Assignments.GetAll().Where(x => Equals(x.MainTask.Id, mainTask.Id));
        foreach (var assign in allAssignments)
        {
          bool likeApprAs = ApprovalAssignments.Is(assign);
          bool likeManApprAs = ApprovalManagerAssignments.Is(assign);

          if ((likeApprAs || likeManApprAs) && assign.Status == Sungero.Workflow.Assignment.Status.InProcess)
            checkApprovalStatus = false;
        }
        if (checkApprovalStatus)
          return "Согласовано";
        else
          return "На согласовании";
      }
    }
    
    [Public(WebApiRequestType = RequestType.Get)]
    public Structures.Module.ICompanyStr GetCompany(string extId)
    {
      var entityDto = Structures.Module.CompanyStr.Create();

      var entity = Companies.GetAll().Where(e => e.ExtIdhprom == extId).FirstOrDefault();
      if (entity != null)
        entityDto = CopyFromEntity(entity);
      
      return entityDto;
    }
    
    #endregion
    
    #region Синхронизация данных
    
    #region ОШС
    
    [Public(WebApiRequestType = RequestType.Post)]
    public Structures.Module.IEmployeeStr SyncEmployee(Structures.Module.IEmployeeStr employeeStr)
    {
      var entityDto = Structures.Module.EmployeeStr.Create();
      
      try
      {
        var extId = employeeStr.ExtId;
        var entity = Employees.GetAll().Where(e => e.ExtIdhprom == extId).FirstOrDefault();
        if (entity == null)
        {
          entity = InsertEmployee(employeeStr);
        }
        else
        {
          entity = UpdateEmployee(entity, employeeStr);
        }
        
        employeeStr.Id = entity.Id;
        employeeStr.ExtId = entity.ExtIdhprom;
        employeeStr.LastName = entity.Person.LastName;
        employeeStr.FirstName = entity.Person.FirstName;
        employeeStr.MiddleName = entity.Person.MiddleName;
        employeeStr.Login = entity.Login?.LoginName;
        employeeStr.Department = entity.DepartmentExtIdhprom;
        employeeStr.Description = entity.Description;
        employeeStr.Email = entity.Email;
        employeeStr.Name = entity.Name;
        employeeStr.IsSystem = entity.IsSystem;
        employeeStr.Person = entity.PersonExtIdhprom;
        employeeStr.JobTitle = entity.JobTitle?.Name;
        employeeStr.Phone = entity.Phone;
        employeeStr.Note = entity.Note;
        employeeStr.NeedNotifyNewAssignments = entity.NeedNotifyNewAssignments;
        employeeStr.NeedNotifyExpiredAssignments = entity.NeedNotifyExpiredAssignments;
        employeeStr.Status = entity.Status?.Value;

        employeeStr.TIN = entity.Person.TIN;
        employeeStr.INILA = entity.Person.INILA;
        employeeStr.DateOfBirth = entity.Person.DateOfBirth?.ToString();
        employeeStr.Sex = entity.Person.Sex?.Value;
        employeeStr.EmployeeNumber = entity.EmployeeNumberhprom;
      }
      catch (Exception exc)
      {
        Logger.Error(" >>> SOFTLINE >>> {exc.Message}, {exc.StackTrace}");
        throw new Exception(exc.Message);
      }
      
      return employeeStr;
    }
    public IEmployee InsertEmployee(Structures.Module.IEmployeeStr inputData)
    {
      try
      {
        var entity = Employees.Create();
        var extId = inputData.ExtId;
        entity.ExtIdhprom = extId;
        entity.PersonExtIdhprom = inputData.Person;
        if (inputData.LastName != null)
          entity.Person.LastName = inputData.LastName;
        if (inputData.FirstName != null)
          entity.Person.FirstName = inputData.FirstName;
        if (inputData.MiddleName != null)
          entity.Person.MiddleName = inputData.MiddleName;
        if (inputData.TIN != null)
          entity.Person.TIN = inputData.TIN;
        if (inputData.INILA != null)
          entity.Person.INILA = inputData.INILA;
        var dateOfBirth = inputData.DateOfBirth;
        if (!string.IsNullOrEmpty(dateOfBirth))
          entity.Person.DateOfBirth = Convert.ToDateTime(dateOfBirth);
        else
          entity.Person.DateOfBirth = null;
        if (inputData.Sex != null)
        {
          if (inputData.Sex == "Male")
            entity.Person.Sex = Sungero.Parties.Person.Sex.Male;
          else if (inputData.Sex == "Female")
            entity.Person.Sex = Sungero.Parties.Person.Sex.Female;
        }
        if (inputData.Status == "Active")
        {
          entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Active;
          entity.Person.Status = Sungero.CoreEntities.DatabookEntry.Status.Active;
        }
        else if (inputData.Status == "Closed")
        {
          entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Closed;
          entity.Login = null;
          entity.Person.Status = Sungero.CoreEntities.DatabookEntry.Status.Closed;
        }
        entity.Person.Save();
        if (inputData.Login != null)
          entity.Login = Sungero.CoreEntities.Logins.GetAll().Where(l => l.LoginName == inputData.Login).FirstOrDefault();
        if (inputData.JobTitle != null)
          entity.JobTitle = Sungero.Company.JobTitles.GetAll().Where(l => l.Name == inputData.JobTitle).FirstOrDefault(); ;
        if (inputData.Department != null)
          entity.DepartmentExtIdhprom = inputData.Department;
        if (inputData.Phone != null)
          entity.Phone = inputData.Phone;
        if (inputData.Note != null)
          entity.Note = inputData.Note;
        if (inputData.Email != null)
          entity.Email = inputData.Email;
        if (inputData.NeedNotifyExpiredAssignments != null)
          entity.NeedNotifyExpiredAssignments = inputData.NeedNotifyExpiredAssignments;
        if (inputData.NeedNotifyNewAssignments != null)
          entity.NeedNotifyNewAssignments = inputData.NeedNotifyNewAssignments;
        entity.AdmManagerExtIdhprom = inputData.AdministrativeManager;
        entity.FuncManagerExtIdhprom = inputData.FunctionalManager;
        entity.Name = entity.Person.Name;
        entity.EmployeeNumberhprom = inputData.EmployeeNumber;
        entity.Save();
        
        var person = entity.Person;
        if (person != null)
        {
          person.Status = entity.Status;
          person.Save();
        }

        return entity;
      }
      catch (Exception exc)
      {
        Logger.Error($" >>> SOFTLINE >>> Ошибка при создании сотрудника с ExtId='{inputData.ExtId}': {exc.Message}, {exc.StackTrace}");
        SendMessage("Логи ошибки отправке сотрудника", inputData.ExtId, exc.Message, exc.StackTrace);
        throw new Exception(exc.Message);
      }
    }
    public IEmployee UpdateEmployee(IEmployee entity, Structures.Module.IEmployeeStr inputData)
    {
      try
      {
        // проверка изменения сущности доступна через entity.State.IsChanged
        // срабатывает true, если затрагивалось изменение.
        // можно проверять значение в сущности с тем, что мы передаем, и перед сохранением единожды проверить, была ли сущность изменена
        bool isUpdateted = false;
        
        //var entityDto = new EmployeeDto();
        if (entity.PersonExtIdhprom != inputData.Person)
        {
          entity.PersonExtIdhprom = inputData.Person;
          isUpdateted = true;
        }
        if (inputData.LastName != null)
        {
          if (entity.Person.LastName != inputData.LastName)
          {
            entity.Person.LastName = inputData.LastName;
            isUpdateted = true;
          }
        }
        if (inputData.FirstName != null)
        {
          if (entity.Person.FirstName != inputData.FirstName)
          {
            entity.Person.FirstName = inputData.FirstName;
            isUpdateted = true;
          }
        }
        if (inputData.MiddleName != null)
        {
          if (entity.Person.MiddleName != inputData.MiddleName)
          {
            entity.Person.MiddleName = inputData.MiddleName;
            isUpdateted = true;
          }
        }
        if (inputData.TIN != null)
        {
          if (entity.Person.TIN != inputData.TIN)
          {
            entity.Person.TIN = inputData.TIN;
            isUpdateted = true;
          }
        }
        if (inputData.INILA != null)
        {
          if (entity.Person.INILA != inputData.INILA)
          {
            entity.Person.INILA = inputData.INILA;
            isUpdateted = true;
          }
        }
        var dateOfBirth = inputData.DateOfBirth;
        if (!string.IsNullOrEmpty(dateOfBirth))
        {
          if (entity.Person.DateOfBirth != Convert.ToDateTime(dateOfBirth))
          {
            entity.Person.DateOfBirth = Convert.ToDateTime(dateOfBirth);
            isUpdateted = true;
          }
        }
        else
        {
          if (entity.Person.DateOfBirth != null)
          {
            entity.Person.DateOfBirth = null;
            isUpdateted = true;
          }
        }
        if (inputData.Sex != null)
        {
          if (inputData.Sex == "Male")
          {
            if (entity.Person.Sex != Sungero.Parties.Person.Sex.Male)
            {
              entity.Person.Sex = Sungero.Parties.Person.Sex.Male;
              isUpdateted = true;
            }
          }
          else if (inputData.Sex == "Female")
          {
            if (entity.Person.Sex != Sungero.Parties.Person.Sex.Female)
            {
              entity.Person.Sex = Sungero.Parties.Person.Sex.Female;
              isUpdateted = true;
            }
          }

        }
        if (inputData.Status == "Active")
        {
          if (entity.Status != Sungero.CoreEntities.DatabookEntry.Status.Active)
          {
            entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Active;
            entity.Person.Status = Sungero.CoreEntities.DatabookEntry.Status.Active;
            isUpdateted = true;
          }
        }
        else if (inputData.Status == "Closed")
        {
          if (entity.Status != Sungero.CoreEntities.DatabookEntry.Status.Closed)
          {
            //Проверка и отправка не выполненых заданий у уволившегося сотрудника
            var allAssignments = Sungero.Workflow.Assignments.GetAll().Where(e => Equals(e.Performer.Id, entity.Id) &&
                                                                             e.Status == Sungero.Workflow.Task.Status.InProcess);
            if (allAssignments.Count() != 0)
            {
              var admManager = Employees.GetAll().Where(e => Equals(e, entity.AdministrativeManagerhprom));
              var funcManager = Employees.GetAll().Where(e =>  Equals(e, entity.FunctionalManagerhprom));
              
              string SMTPServer = "smtp.office365.com";
              int SMTPPort = 587;
              string password = "z@Q2Mt4n^oZGUu$Z";
              string mailFrom = "directum-robot@hlebprom.com";
              string defaultMail = "shirokov@hlebprom.com";// TODO ShirokovHP Пересмотреть в релизе
              string mailTo = "";

              if (admManager.FirstOrDefault() != null)
                mailTo = admManager.FirstOrDefault().Email;

              if (mailTo == "")
                mailTo = defaultMail;

              MailAddress from = new MailAddress(mailFrom);
              MailAddress to = new MailAddress(mailTo);

              SmtpClient client = new SmtpClient(SMTPServer, SMTPPort);
              client.Credentials = new NetworkCredential(mailFrom, password);
              var empl = allAssignments.FirstOrDefault().Performer;
              ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
              client.EnableSsl = true;

              MailMessage message = new MailMessage(from, to);
              string body = "У сотрудника " + empl.Name + " есть незавершенные задания.\n";
              body += "Обратитесь в тех поддержку для настройки Замещения, уволившегося сотрудника.\n";
              body += "Список незавершенных заданий:\n";
              string subject = "Список не законченных заданий уволившегося сотрудника: " + empl.Name;
              foreach (var item in allAssignments)
              {
                body += "ИД: " + item.Id + "  " + item.Subject + "\n";
              }
              message.Body = body;//Добавить html форматирован
              message.BodyEncoding = System.Text.Encoding.UTF8;
              message.Subject = subject;
              message.SubjectEncoding = System.Text.Encoding.UTF8;

              if (funcManager.FirstOrDefault() != null)
                if (funcManager.FirstOrDefault().Email != "" && funcManager.FirstOrDefault() != admManager.FirstOrDefault())
                  message.To.Add(funcManager.FirstOrDefault().Email);

              client.Send(message);
            }
            entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Closed;
            entity.Login = null;
            entity.Person.Status = Sungero.CoreEntities.DatabookEntry.Status.Closed;
            isUpdateted = true;
          }
        }
        if (isUpdateted)
        {
          entity.Person.Save();
        }
        if (inputData.Login != null)
        {
          var login = Sungero.CoreEntities.Logins.GetAll().Where(l => l.LoginName == inputData.Login).FirstOrDefault();
          if (entity.Login != login)
          {
            entity.Login = login;
            isUpdateted = true;
          }
        }
        if (inputData.JobTitle != null)
        {
          var jobTitle = Sungero.Company.JobTitles.GetAll().Where(l => l.Name == inputData.JobTitle).FirstOrDefault();
          if (entity.JobTitle != jobTitle)
          {
            entity.JobTitle = jobTitle;
            isUpdateted = true;
          }
        }
        if (inputData.Department != null)
        {
          if (entity.DepartmentExtIdhprom != inputData.Department)
          {
            entity.DepartmentExtIdhprom = inputData.Department;
            isUpdateted = true;
          }
        }
        if (inputData.Phone != null)
        {
          if (entity.Phone != inputData.Phone)
          {
            entity.Phone = inputData.Phone;
            isUpdateted = true;
          }
        }
        if (inputData.Note != null)
        {
          if (entity.Note != inputData.Note)
          {
            entity.Note = inputData.Note;
            isUpdateted = true;
          }
        }
        if (inputData.Email != null)
        {
          if (entity.Email != inputData.Email)
          {
            entity.Email = inputData.Email;
            isUpdateted = true;
          }
        }
        if (inputData.NeedNotifyExpiredAssignments != null)
        {
          if (entity.NeedNotifyExpiredAssignments != inputData.NeedNotifyExpiredAssignments)
          {
            entity.NeedNotifyExpiredAssignments = inputData.NeedNotifyExpiredAssignments;
            isUpdateted = true;
          }
        }
        if (inputData.NeedNotifyNewAssignments != null)
        {
          if (entity.NeedNotifyNewAssignments != inputData.NeedNotifyNewAssignments)
          {
            entity.NeedNotifyNewAssignments = inputData.NeedNotifyNewAssignments;
            isUpdateted = true;
          }
        }
        if (entity.AdmManagerExtIdhprom != inputData.AdministrativeManager)
        {
          entity.AdmManagerExtIdhprom = inputData.AdministrativeManager;
          isUpdateted = true;
        }
        if (entity.FuncManagerExtIdhprom != inputData.FunctionalManager)
        {
          entity.FuncManagerExtIdhprom = inputData.FunctionalManager;
          isUpdateted = true;
        }
        if (entity.Name != entity.Person.Name)
        {
          entity.Name = entity.Person.Name;
          isUpdateted = true;
        }
        if (inputData.EmployeeNumber != null)
        {
          if (entity.EmployeeNumberhprom != inputData.EmployeeNumber)
          {
            entity.EmployeeNumberhprom = inputData.EmployeeNumber;
            isUpdateted = true;
          }
        }
        if (isUpdateted)
        {
          //entity.LastUpdate = DateTime.Today();
          entity.Save();
        }

        return entity;
      }
      catch (Exception exc)
      {
        Logger.Error($" >>> SOFTLINE >>> Ошибка при обновлении сотрудника с ExtId='{inputData.ExtId}': {exc.Message}, {exc.StackTrace}");
        SendMessage("Логи ошибки отправке сотрудника", inputData.ExtId, exc.Message, exc.StackTrace);
        throw new Exception(exc.Message);
      }
    }
    
    [Public(WebApiRequestType = RequestType.Post)]
    public Structures.Module.IJobTitleStr SyncJobTitle(Structures.Module.IJobTitleStr jobTitleStr)
    {
      var name = jobTitleStr.Name;
      var entity = JobTitles.GetAll().FirstOrDefault(e => e.Name == name) ?? JobTitles.Create();

      entity.Name = jobTitleStr.Name;
      entity.ExtIdhprom = jobTitleStr.ExtId;

      if (jobTitleStr.Status == "Active")
        entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Active;
      else if (jobTitleStr.Status == "Closed")
        entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Closed;

      entity.Save();
      
      jobTitleStr.Id = entity.Id;
      jobTitleStr.ExtId = entity.ExtIdhprom;
      jobTitleStr.Name = entity.Name;
      jobTitleStr.Status = entity.Status?.Value;
      
      return jobTitleStr;
    }
    
    [Public(WebApiRequestType = RequestType.Post)]
    public Structures.Module.IDepartmentStr SyncDepartment(Structures.Module.IDepartmentStr departmentStr)
    {
      try
      {
        var extId = departmentStr.ExtId;
        var entity = Departments.GetAll().Where(e => e.ExtIdhprom == extId).FirstOrDefault() ?? Departments.Create();

        entity.ExtIdhprom = extId;

        entity.Name = departmentStr.Name;
        if (departmentStr.Manager != null)
          entity.ManagerExtIdhprom = departmentStr.Manager;

        if (departmentStr.HeadOffice != null)
          entity.HeadOfficeExtIdhprom = departmentStr.HeadOffice;

        if (departmentStr.Phone != null)
          entity.Phone = departmentStr.Phone;

        if (departmentStr.ShortName != null)
          entity.ShortName = departmentStr.ShortName;

        if (departmentStr.Note != null)
          entity.Note = departmentStr.Note;

        if (departmentStr.BusinessUnit != null)
          entity.BusinessUnitExtIdhprom = departmentStr.BusinessUnit;

        if (departmentStr.Code != null)
          entity.Code = departmentStr.Code;

        if (departmentStr.Status == "Active")
          entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Active;
        else if (departmentStr.Status == "Closed")
          entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Closed;

        entity.Save();
        
        departmentStr.Id = entity.Id;
        departmentStr.ExtId = entity.ExtIdhprom;
        departmentStr.Sid = entity.Sid;
        departmentStr.Name = entity.Name;
        departmentStr.Description = entity.Description;
        departmentStr.IsSystem = entity.IsSystem;
        departmentStr.Manager = entity.ManagerExtIdhprom;
        departmentStr.HeadOffice = entity.HeadOfficeExtIdhprom;
        departmentStr.Phone = entity.Phone;
        departmentStr.ShortName = entity.ShortName;
        departmentStr.Note = entity.Note;
        departmentStr.BusinessUnit = entity.BusinessUnitExtIdhprom;
        departmentStr.Code = entity.Code;
        departmentStr.Status = entity.Status.ToString();

        return departmentStr;
      }
      catch (Exception exc)
      {
        Logger.Error($" >>> SOFTLINE >>> '{exc.Message}', {exc.StackTrace}");
        throw new Exception(exc.Message);
      }
    }
    
    [Public(WebApiRequestType = RequestType.Post)]
    public Structures.Module.ILoginStr SyncLogin(Structures.Module.ILoginStr loginStr)
    {
      try
      {
        var loginName = loginStr.LoginName;
        var entity = Logins.GetAll().Where(l => l.LoginName == loginName).FirstOrDefault() ?? Logins.Create();

        entity.LoginName = loginName;
        
        if (loginStr.TypeAuthentication == "Windows")
          entity.TypeAuthentication = Sungero.CoreEntities.Login.TypeAuthentication.Windows;
        else if (loginStr.TypeAuthentication == "Password")
          entity.TypeAuthentication = Sungero.CoreEntities.Login.TypeAuthentication.Password;

        entity.NeedChangePassword = loginStr.NeedChangePassword;

        if (loginStr.Status == "Active")
          entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Active;
        else if (loginStr.Status == "Closed")
          entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Closed;

        entity.Save();
        
        loginStr.Id = entity.Id;
        loginStr.LoginName = entity.LoginName;
        loginStr.TypeAuthentication = entity.TypeAuthentication.ToString();
        loginStr.NeedChangePassword = entity.NeedChangePassword;
        loginStr.Status = entity.Status.ToString();
        
        return loginStr;
      }
      catch (Exception exc)
      {
        Logger.Error($" >>> SOFTLINE >>> '{exc.Message}', {exc.StackTrace}");
        throw new Exception(exc.Message);
      }
    }
    
    #endregion
    
    [Public(WebApiRequestType = RequestType.Post)]
    public Structures.Module.IAcquaintanceTaskStr CreateAcquaintanceTask(Structures.Module.IAcquaintanceTaskStr acquaintanceTaskStr)
    {
      string employeeExtId = "testData";
      string errorMSG = string.Empty;
      int doc = 0;
      try
      {
        employeeExtId = acquaintanceTaskStr.EmployeeExtId;
        doc = acquaintanceTaskStr.DocumentId;

        var employee = Employees.GetAll().Where(e => e.ExtIdhprom == employeeExtId).FirstOrDefault();
        if (employee == null)
        {
          errorMSG = $"Employee {employeeExtId} not founded";
          Logger.Error(errorMSG);
        }
        
        // при попытке получения через get появится ошибка, если не существует документа с таким ид
        var document = Sungero.Docflow.OfficialDocuments.Null;
        try
        {
          document = Sungero.Docflow.OfficialDocuments.Get(doc);
          if (document == null)
          {
            if (errorMSG == "InputDataOK")
              errorMSG = string.Empty;
            errorMSG += $"\nDocument {doc} not founded";
            Logger.Error(errorMSG);
          }
        }
        catch
        {
          if (errorMSG == "InputDataOK")
            errorMSG = string.Empty;
          errorMSG += $"\nDocument {doc} not founded";
          Logger.Error(errorMSG);
        }

        var acqTask = Sungero.RecordManagement.AcquaintanceTasks.Create();
        if (document != null && document.HasVersions && employee != null)
        {
          // TODO RomanovSL вынужденная мера, т.к. задачу на ознакомление нельзя запускать от имени администратора
          acqTask.Author = employee;
          acqTask.DocumentGroup.All.Add(document);
          acqTask.Performers.AddNew().Performer = employee;
          acqTask.Deadline = Calendar.Now.AddDays(10);
          acqTask.Save();

          acqTask.Start();
          acquaintanceTaskStr.Subject = acqTask.Subject;
          acquaintanceTaskStr.Deadline = acqTask.Deadline;
          acquaintanceTaskStr.Author = acqTask.Author.Name;
          acquaintanceTaskStr.Status = acqTask.Status.ToString();
          acquaintanceTaskStr.Importance = acqTask.Importance.ToString();
          acquaintanceTaskStr.Created = acqTask.Created;
          acquaintanceTaskStr.Id = acqTask.Id;
          //acquaintanceTaskStr.MainTask = acqTask.MainTask.Id;
        }

        return acquaintanceTaskStr;
      }
      catch (Exception exc)
      {
        //SendMessage("Логи ошибки при отправке задачи на ознакомление",employeeExtId, doc.ToString(),errorMSG, exc.Message, exc.StackTrace);
        Logger.Error($" >>> SOFTLINE >>> '{exc.Message}', {exc.StackTrace}");
        throw new Exception(exc.Message + exc.StackTrace);
      }
    }
        
    [Public(WebApiRequestType = RequestType.Post)]
    public Structures.Module.ICityStr SyncCity(Structures.Module.ICityStr cityStr)
    {
      var extId = cityStr.ExtId;
      var entity = Cities.GetAll().FirstOrDefault(e => e.Region.Code == cityStr.Region && e.Name.Contains(cityStr.Name)) ?? Cities.Create();
      entity.Name = cityStr.Name;
      entity.Region = Sungero.Commons.Regions.GetAll().Where(r => r.Code == cityStr.Region).FirstOrDefault();
      entity.Country = Sungero.Commons.Countries.GetAll().Where(r => r.Code == cityStr.Region).FirstOrDefault();

      if (cityStr.Status == "Active")
        entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Active;
      else if (cityStr.Status == "Closed")
        entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Closed;

      entity.Save();
      
      cityStr.Id = entity.Id;
      cityStr.ExtId = entity.ExtIdhprom;
      cityStr.Name = entity.Name;
      cityStr.Region = entity.Region.Code;
      cityStr.Country = entity.Country.Code;
      cityStr.Status = entity.Status.ToString();

      return cityStr;
    }
    
    [Public(WebApiRequestType = RequestType.Post)]
    public Structures.Module.ICompanyStr SyncCompany(Structures.Module.ICompanyStr companyStr)
    {
      try
      {
        var extId = companyStr.ExtId;
        var entity = Companies.GetAll().Where(e => e.ExtIdhprom == extId).FirstOrDefault() ?? Companies.Create();

        entity.ExtIdhprom = extId;
        entity.Name = companyStr.Name;
        entity.TIN = companyStr.TIN;
        entity.TRRC = companyStr.TRRC;
        entity.PSRN = companyStr.PSRN;
        entity.NCEA = companyStr.NCEA;
        entity.NCEO = companyStr.NCEO;
        entity.ResponsibleExtIdhprom = companyStr.Responsible;
        entity.CityExtIdhprom = companyStr.City;
        entity.Region = Sungero.Commons.Regions.GetAll().Where(r => r.Code == companyStr.Region).FirstOrDefault();
        entity.LegalAddress = companyStr.LegalAddress;
        entity.PostalAddress = companyStr.PostalAddress;
        entity.Phones = companyStr.Phones;
        entity.Email = companyStr.Email;
        entity.Homepage = companyStr.Homepage;
        entity.LegalName = companyStr.LegalName;
        entity.HeadCompanyExtIdhprom = companyStr.HeadCompany;
        entity.Nonresident = companyStr.Nonresident;
        entity.Account = companyStr.Account;
        entity.Bank = Sungero.Parties.Banks.GetAll().Where(r => r.BIC == companyStr.Bank).FirstOrDefault();
        entity.Note = companyStr.Note;
        entity.Code = companyStr.Code;

        if (companyStr.Status == "Active")
          entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Active;
        else if (companyStr.Status == "Closed")
          entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Closed;

        entity.Save();
        
        return CopyFromEntity(entity);
      }
      catch (Exception exc)
      {
        Logger.Error($" >>> SOFTLINE >>> '{exc.Message}', {exc.StackTrace}");
        throw new Exception(exc.Message);
      }
    }
    public Structures.Module.ICompanyStr CopyFromEntity(ICompany entity)
    {
      var companyStr = Structures.Module.CompanyStr.Create();
      
      companyStr.Id = entity.Id;
      companyStr.ExtId = entity.ExtIdhprom;
      companyStr.Name = entity.Name;
      companyStr.TIN = entity.TIN;
      companyStr.TRRC = entity.TRRC;
      companyStr.PSRN = entity.PSRN;
      companyStr.NCEO = entity.NCEO;
      companyStr.NCEA = entity.NCEA;
      companyStr.Responsible = entity.ResponsibleExtIdhprom;
      companyStr.City = entity.City?.Name;
      companyStr.Region = entity.Region?.Code;
      companyStr.LegalAddress = entity.LegalAddress;
      companyStr.PostalAddress = entity.PostalAddress;
      companyStr.Phones = entity.Phones;
      companyStr.Email = entity.Email;
      companyStr.Homepage = entity.Homepage;
      companyStr.LegalName = entity.LegalName;
      companyStr.HeadCompany = entity.HeadCompanyExtIdhprom;
      companyStr.Nonresident = entity.Nonresident;
      companyStr.Account = entity.Account;
      companyStr.Bank = entity.Bank?.BIC;
      companyStr.Note = entity.Note;
      companyStr.Status = entity.Status?.Value;
      companyStr.Code = entity.Code;
      
      return companyStr;
    }
    
    [Public(WebApiRequestType = RequestType.Post)]
    public Structures.Module.IContactStr SyncContact(Structures.Module.IContactStr contactStr)
    {
      try
      {
        var extId = contactStr.ExtId;
        var entity = Contacts.GetAll().Where(e => e.ExtIdhprom == extId).FirstOrDefault() ?? Contacts.Create();

        entity.ExtIdhprom = extId;
        entity.Name = contactStr.Name;
        entity.CompanyExtIdhprom = contactStr.Company;
        entity.Department = contactStr.Department;
        entity.JobTitle = contactStr.JobTitle;
        entity.Phone = contactStr.Phone;
        entity.Fax = contactStr.Fax;
        entity.Email = contactStr.Email;
        entity.Homepage = contactStr.Homepage;
        entity.Note = contactStr.Note;

        if (contactStr.Status == "Active")
          entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Active;
        else if (contactStr.Status == "Closed")
          entity.Status = Sungero.CoreEntities.DatabookEntry.Status.Closed;

        entity.Save();
        
        contactStr.Id = entity.Id;
        contactStr.Name = entity.Name;
        contactStr.Person = entity.PersonExtIdhprom;
        contactStr.Company = entity.CompanyExtIdhprom;
        contactStr.Department = entity.Department;
        contactStr.JobTitle = entity.JobTitle;
        contactStr.Phone = entity.Phone;
        contactStr.Fax = entity.Fax;
        contactStr.Email = entity.Email;
        contactStr.Note = entity.Note;
        contactStr.Homepage = entity.Homepage;
        contactStr.Status = entity.Status?.Value;
        
        return contactStr;
      }
      catch (Exception exc)
      {
        Logger.Error($" >>> SOFTLINE >>> '{exc.Message}', {exc.StackTrace}");
        throw new Exception(exc.Message);
      }
    }
    
    #region не работает, позже выяснить почему
    /*
    [Public(WebApiRequestType = RequestType.Post)]
    public bool CreateDocOrder(Structures.Module.IDocStr docStr)
    {
      var entityDto = Structures.Module.DocStr.Create();
      string message = string.Empty;
      try
      {
        message = ValidateStructure(docStr);
        if (!string.IsNullOrEmpty(message))
        {
          Logger.Error($" >>> SOFTLINE >>> {message}");
          throw new Exception(message);
        }
        var order = Orders.Create();
        order.DocumentKind = DocumentKinds.GetAll().Where(x => x.Name == docStr.DocumentKind).FirstOrDefault();
        var author = Employees.GetAll().Where(x => x.ExtIdhprom == docStr.AuthorExtId).FirstOrDefault();
        order.Author = author;
        order.BusinessUnit = BusinessUnits.GetAll().Where(x => x.ExtIdhprom == docStr.BusinessUnit).FirstOrDefault();
        order.Department = Departments.GetAll().Where(x => Equals(x.Id, author.Department.Id)).FirstOrDefault();
        order.DocumentDate = Calendar.Now;
        order.Assignee = Employees.GetAll().Where(x => x.ExtIdhprom == docStr.EmployeeExtId).FirstOrDefault();
        Calendar.TryParseDate(docStr.DocumentDate, out DateTime date);
        order.Subject = "ИД " + order.Id + " , " + order.DocumentKind.Name + " № \"" + docStr.DocumentNumber + "\" от " +
          date.ToString("dd.MM.yyyy") + " на сотрудника - " + order.Assignee.DisplayValue;
        order.DisplayValue = order.Subject;
        order.Responsiblehprom = author.Name;
        order.Responsibleshprom.AddNew().ResponsibleProperty = author;
        order.OneTimeDochprom = true;
        order.DepartmentsAffectedPromhprom.AddNew().Department = order.Department;
        order.ESignhprom = true;
        order.PreparedBy = author;
        
        using (MemoryStream stream = new MemoryStream())
        {
          var app = Sungero.Content.AssociatedApplications.GetAll().Where(x => x.Extension == docStr.DocExt).FirstOrDefault();
          byte[] data = Convert.FromBase64String(docStr.LastVersionBody);
          stream.Write(data, 0, data.Length);
          order.CreateVersion();
          order.LastVersion.AssociatedApplication = app;
          order.LastVersion.Body.Write(stream);
        }
        order.Save();

        var leadingDoc = Sungero.Docflow.OfficialDocuments.GetAll().Where(x => Equals(x.Id.ToString(), docStr.LeadingDocumentId)).FirstOrDefault();
        if (leadingDoc == null)
        {
          message = string.Format("Ведущий документ ИД {0} для приказа {1} ИД {2} не найден", docStr.LeadingDocumentId, docStr.DocumentKind, order.Id);
          Logger.Debug($" >>> SOFTLINE >>> {message}");
          SendNotify(author.Person.Email, message);
        }
        else
        {
          leadingDoc.Relations.Add("Basis", order);
          leadingDoc.Save();
        }
        var task = Sungero.Docflow.PublicFunctions.Module.Remote.CreateApprovalTask(order);
        if (docStr.Trace)
        {
          SendTrace($"Task: {task.Id}");
          Logger.Debug($" >>> SOFTLINE >>> Task: {task.Id}");
        }
        if (task == null)
        {
          message = "(ApprovalTask == null)";
          Logger.Error($" >>> SOFTLINE >>> {message}");
          throw new Exception(message);
        }
        if (task.ApprovalRule == null)
        {
          var rule = Sungero.Docflow.ApprovalRuleBases.GetAll().Where(x => x.Name == docStr.ApprovalRule).FirstOrDefault();
          if (rule == null)
          {
            message = string.Format("Not found ApprovalRule: {0}", docStr.ApprovalRule);
            Logger.ErrorFormat("Not found ApprovalRule: {0}", docStr.ApprovalRule);
            throw new Exception(message);
          }
          task.ApprovalRule = rule;
          if (docStr.Trace)
          {
            Logger.Debug($" >>> SOFTLINE >>> ApprovalRule: {task.ApprovalRule.Name}");
            SendTrace($"ApprovalRule: {task.ApprovalRule.Name}");
          }
        }
        task.Signatory = Employees.GetAll().Where(x => x.ExtIdhprom == docStr.Signatory).FirstOrDefault();
        task.Author = Employees.GetAll().Where(x => x.ExtIdhprom == docStr.EmployeeExtId).FirstOrDefault();
        //task.Save();
        try
        {
          Sungero.Workflow.Functions.TaskRemoteFunctions.Start(task);
        }
        catch (Exception ex)
        {
          Logger.Error($" >>> SOFTLINE >>> TaskRemoteFunctions.Start() '{ex.Message}', {ex.StackTrace}");
          SendTrace($"TaskRemoteFunctions.Start() {ex.Message}\n{ex.StackTrace}");
        }
        
        return true;
      }
      catch (Exception ex)
      {
        SendException(ex.Message, ex.StackTrace);
        Logger.Error($" >>> SOFTLINE >>> TaskRemoteFunctions.Start() '{ex.Message}', {ex.StackTrace}");
        return false;
      }

    }
    public string ValidateStructure(Structures.Module.IDocStr inputData)
    {
      string message = string.Empty;
      try
      {
        var signatory = Employees.GetAll().Where(x => x.ExtIdhprom == inputData.Signatory).FirstOrDefault();
        if (signatory == null)
        {
          message += string.Format("Не найден подписант Signatory: {0} \r\n", inputData.Signatory);
        }
        else
        {
          var recipient = Sungero.Docflow.SignatureSettings.GetAll().Where(x => Equals(x.Recipient.Id, signatory.Id)).FirstOrDefault();
          if (recipient == null)
          {
            message += string.Format("Нет права подписи у Signatory: {0} \r\n", signatory.Name);
          }
        }
        var DocumentKind = DocumentKinds.GetAll().Where(x => x.Name == inputData.DocumentKind).FirstOrDefault();
        if (DocumentKind == null)
        {
          message += string.Format("Не найден вид документа DocumentKind: {0} \r\n", inputData.DocumentKind);
        }
        var author = Employees.GetAll().Where(x => x.ExtIdhprom == inputData.AuthorExtId).FirstOrDefault();
        if (author == null)
        {
          message += string.Format("Не найден автор документа AuthorExtId: {0} \r\n", inputData.AuthorExtId);
        }
        var businessUnit = BusinessUnits.GetAll().Where(x => x.ExtIdhprom == inputData.BusinessUnit).FirstOrDefault();
        if (businessUnit == null)
        {
          message += string.Format("Не найден BusinessUnit: {0} \r\n", inputData.BusinessUnit);
        }
        var assignee = Employees.GetAll().Where(x => x.ExtIdhprom == inputData.EmployeeExtId).FirstOrDefault();
        if (assignee == null)
        {
          message += string.Format("Не найден сотрудник EmployeeExtId: {0} \r\n", inputData.EmployeeExtId);
        }
        
        if (!Calendar.TryParseDate(inputData.DocumentDate, out DateTime date))
        {
          message += String.Format("Некорректная дата документа: {0} \r\n", inputData.DocumentDate);
        }
        return message;
      }
      catch (Exception ex)
      {
        message = ex.Message;
        SendException(ex.Message, ex.StackTrace);
        return ex.Message;
      }

    }
     */
    #endregion
    
    #endregion
    
    #region Отправка уведомлений в почту
    public void SendException(string exMessage, string exStackTrace)
    {
      return;
      int SMTPPort = 587;
      string SMTPServer = "smtp.office365.com";
      string mailFrom = "directum-robot@hlebprom.com";
      string mailTo = "limin@hlebprom.com";
      string password = "z@Q2Mt4n^oZGUu$Z";
      SmtpClient client = new SmtpClient(SMTPServer, SMTPPort);
      client.Credentials = new NetworkCredential(mailFrom, password);
      MailMessage message = new MailMessage(mailFrom, mailTo);
      message.To.Add("shirokov@hlebprom.com");
      message.Body += exMessage + "<br><br>";
      message.Body += exStackTrace + "<br><br>";
      message.IsBodyHtml = true;
      message.BodyEncoding = System.Text.Encoding.UTF8;
      message.Subject = "IntegrationService Exception";
      message.SubjectEncoding = System.Text.Encoding.UTF8;
      System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
      client.EnableSsl = true;
      client.Send(message);
      client.Dispose();

    }
    public void SendNotify(string mailTo, string msg)
    {
      return;
      int SMTPPort = 587;
      string SMTPServer = "smtp.office365.com";
      string mailFrom = "directum-robot@hlebprom.com";
      string password = "z@Q2Mt4n^oZGUu$Z";
      SmtpClient client = new SmtpClient(SMTPServer, SMTPPort);
      client.Credentials = new NetworkCredential(mailFrom, password);
      MailMessage message = new MailMessage(mailFrom, mailTo);
      message.Body = msg;
      message.IsBodyHtml = true;
      message.BodyEncoding = System.Text.Encoding.UTF8;
      message.Subject = "Уведомление от Directum RX";
      message.SubjectEncoding = System.Text.Encoding.UTF8;
      System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
      client.EnableSsl = true;
      client.Send(message);
      client.Dispose();
    }
    public void SendMessage(params string[] msgs)
    {
      return;
      int SMTPPort = 587;
      string SMTPServer = "smtp.office365.com";
      string mailFrom = "directum-robot@hlebprom.com";
      string mailTo = "limin@hlebprom.com";
      string password = "z@Q2Mt4n^oZGUu$Z";

      SmtpClient client = new SmtpClient(SMTPServer, SMTPPort);
      client.Credentials = new NetworkCredential(mailFrom, password);

      MailMessage message = new MailMessage(mailFrom, mailTo);
      message.To.Add("shirokov@hlebprom.com");

      foreach (string str in msgs)
      {
        message.Body += str + "<br><br>";
      }
      message.IsBodyHtml = true;
      message.BodyEncoding = System.Text.Encoding.UTF8;
      message.Subject = "Логи IntegrationService";
      message.SubjectEncoding = System.Text.Encoding.UTF8;
      System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
      client.EnableSsl = true;
      client.Send(message);
      client.Dispose();
    }
    public void SendTrace(string msg)
    {
      return;
      int SMTPPort = 587;
      string SMTPServer = "smtp.office365.com";
      string mailFrom = "directum-robot@hlebprom.com";
      string mailTo = "limin@hlebprom.com";
      string password = "z@Q2Mt4n^oZGUu$Z";
      SmtpClient client = new SmtpClient(SMTPServer, SMTPPort);
      client.Credentials = new NetworkCredential(mailFrom, password);
      MailMessage message = new MailMessage(mailFrom, mailTo);
      message.To.Add("shirokov@hlebprom.com");
      message.Body = msg;
      message.IsBodyHtml = true;
      message.BodyEncoding = System.Text.Encoding.UTF8;
      message.Subject = "IntegrationService Trace";
      message.SubjectEncoding = System.Text.Encoding.UTF8;
      System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
      client.EnableSsl = true;
      client.Send(message);
      client.Dispose();

    }
    #endregion
    
  }
}
