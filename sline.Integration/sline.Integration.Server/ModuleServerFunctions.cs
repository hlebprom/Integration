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
    public Structures.Module.IContractStr CopyFromEntity(hprom.DocflowExt.IContract entity)
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
    public static string GetContractStatus(int idDoc)
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
    
    [Public(WebApiRequestType = RequestType.Post)]
    public bool CreateOrder(Structures.Module.IDocumentStr documentStr)
    {
      var entityDto = Structures.Module.DocumentStr.Create();
      string message = null;
      try
      {
        message = Validate(documentStr);
        if (message != null)
        {
          Logger.Error($" >>> SOFTLINE >>> {message}");
          throw new Exception(message);
        }
        var order = Orders.Create();
        order.DocumentKind = DocumentKinds.GetAll().Where(x => x.Name == documentStr.DocumentKind).FirstOrDefault();
        var author = Employees.GetAll().Where(x => x.ExtIdhprom == documentStr.AuthorExtId).FirstOrDefault();
        order.Author = author;
        order.BusinessUnit = BusinessUnits.GetAll().Where(x => x.ExtIdhprom == documentStr.BusinessUnit).FirstOrDefault();
        order.Department = Departments.GetAll().Where(x => Equals(x.Id, author.Department.Id)).FirstOrDefault();
        order.DocumentDate = Calendar.Now;
        order.Assignee = Employees.GetAll().Where(x => x.ExtIdhprom == documentStr.EmployeeExtId).FirstOrDefault();
        Calendar.TryParseDate(documentStr.DocumentDate, out DateTime date);
        order.Subject = "ИД " + order.Id + " , " + order.DocumentKind.Name + " № \"" + documentStr.DocumentNumber + "\" от " +
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
          var app = Sungero.Content.AssociatedApplications.GetAll().Where(x => x.Extension == documentStr.DocExt).FirstOrDefault();
          byte[] data = Convert.FromBase64String(documentStr.LastVersionBody);
          stream.Write(data, 0, data.Length);
          order.CreateVersion();
          order.LastVersion.AssociatedApplication = app;
          order.LastVersion.Body.Write(stream);
        }
        order.Save();

        var leadingDoc = Sungero.Docflow.OfficialDocuments.GetAll().Where(x => Equals(x.Id.ToString(), documentStr.LeadingDocumentId)).FirstOrDefault();
        if (leadingDoc == null)
        {
          message = string.Format("Ведущий документ ИД {0} для приказа {1} ИД {2} не найден", documentStr.LeadingDocumentId, documentStr.DocumentKind, order.Id);
          Logger.Debug($" >>> SOFTLINE >>> {message}");
          SendNotify(author.Person.Email, message);
        }
        else
        {
          leadingDoc.Relations.Add("Basis", order);
          leadingDoc.Save();
        }
        var task = Sungero.Docflow.PublicFunctions.Module.Remote.CreateApprovalTask(order);
        if (documentStr.Trace)
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
          var rule = Sungero.Docflow.ApprovalRuleBases.GetAll().Where(x => x.Name == documentStr.ApprovalRule).FirstOrDefault();
          if (rule == null)
          {
            message = string.Format("Not found ApprovalRule: {0}", documentStr.ApprovalRule);
            Logger.ErrorFormat("Not found ApprovalRule: {0}", documentStr.ApprovalRule);
            throw new Exception(message);
          }
          task.ApprovalRule = rule;
          if (documentStr.Trace)
          {
            Logger.Debug($" >>> SOFTLINE >>> ApprovalRule: {task.ApprovalRule.Name}");
            SendTrace($"ApprovalRule: {task.ApprovalRule.Name}");
          }
        }
        task.Signatory = Employees.GetAll().Where(x => x.ExtIdhprom == documentStr.Signatory).FirstOrDefault();
        task.Author = Employees.GetAll().Where(x => x.ExtIdhprom == documentStr.EmployeeExtId).FirstOrDefault();
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
        SendException(ex);
        Logger.Error($" >>> SOFTLINE >>> TaskRemoteFunctions.Start() '{ex.Message}', {ex.StackTrace}");
        return false;
      }

    }
    public string Validate(Structures.Module.IDocumentStr inputData)
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
        //MailController.SendException(ex);
        return ex.Message;
      }

    }

    #region Отправка уведомлений в почту
    public void SendException(Exception ex)
    {
      int SMTPPort = 587;
      string SMTPServer = "smtp.office365.com";
      string mailFrom = "directum-robot@hlebprom.com";
      string mailTo = "limin@hlebprom.com";
      string password = "z@Q2Mt4n^oZGUu$Z";
      SmtpClient client = new SmtpClient(SMTPServer, SMTPPort);
      client.Credentials = new NetworkCredential(mailFrom, password);
      MailMessage message = new MailMessage(mailFrom, mailTo);
      message.To.Add("shirokov@hlebprom.com");
      message.Body += ex.Message + "<br><br>";
      message.Body += ex.StackTrace + "<br><br>";
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
