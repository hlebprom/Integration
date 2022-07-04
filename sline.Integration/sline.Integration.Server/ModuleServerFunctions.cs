using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Net.Mail;
using System.Net;

namespace sline.Integration.Server
{
  public class ModuleFunctions
  {
    [Public(WebApiRequestType = RequestType.Post)]
    public Structures.Module.IAcquaintanceTaskStr CreateLogin(Structures.Module.IAcquaintanceTaskStr acquaintanceTaskStr)
    {
      string employeeExtId = "testData";
      string errorMSG = string.Empty;
      int doc = 0;
      try
      {
        employeeExtId = acquaintanceTaskStr.EmployeeExtId;
        doc = acquaintanceTaskStr.DocumentId;

        var employee = hprom.DocflowExt.Employees.GetAll().Where(e => e.ExtIdhprom == employeeExtId).FirstOrDefault();
        if (employee == null)
        {
          errorMSG = $"Employee {employeeExtId} not founded";
          Logger.Error(errorMSG);
        }
        var document = Sungero.Docflow.OfficialDocuments.Get(doc);//All(x => x.Id == doc).First();
        if (document == null)
        {
          if (errorMSG == "InputDataOK")
            errorMSG = "";
          errorMSG += $"\nDocument {doc} not founded";
          Logger.Error(errorMSG);
        }

        var acqTask = Sungero.RecordManagement.AcquaintanceTasks.Create();
        if (document != null && document.HasVersions && employee != null)
        {
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
        }

        return acquaintanceTaskStr;
      }
      catch (Exception exc)
      {
        SendMessage("Логи ошибки при отправке задачи на ознакомление",employeeExtId, doc.ToString(),errorMSG, exc.Message, exc.StackTrace);
        Logger.Error(exc.Message + exc.StackTrace);
        throw new Exception(exc.Message + exc.StackTrace);
      }
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
      message.Subject = "Логи WebAPI";
      message.SubjectEncoding = System.Text.Encoding.UTF8;
      System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
      client.EnableSsl = true;
      client.Send(message);
      client.Dispose();
    }
  }
}
