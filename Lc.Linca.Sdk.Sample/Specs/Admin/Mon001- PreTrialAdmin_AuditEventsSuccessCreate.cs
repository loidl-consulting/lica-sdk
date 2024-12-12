/***********************************************************************************
 * Project:   Linked Care AP5
 * Component: LINCA FHIR SDK and Demo Client
 * Copyright: 2023 LOIDL Consulting & IT Services GmbH
 * Authors:   Annemarie Goldmann, Daniel Latikaynen
 * Purpose:   Sample code to test LINCA and template for client prototypes
 * Licence:   BSD 3-Clause
 * ---------------------------------------------------------------------------------
 * The Linked Care project is co-funded by the Austrian FFG
 ***********************************************************************************/

using Hl7.Fhir.Model;
using System.Globalization;

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class Monitoring001_PreTrialAdmin_AuditEvents_Success_Create : Spec
{
    public const string UserStory = @"
    Monitoring 001: Successful create events in the last 2 days;
    ";

    protected MedicationRequest medReq = new();

    public Monitoring001_PreTrialAdmin_AuditEvents_Success_Create(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Get AuditEvents for successful create request", GetAuditEventsCreate),
            //new("Get AuditEvents for successful GET prescription/s requests", GetAuditEventsGetPrescr),
            //new("Get AuditEvents for erroneous requests", GetAuditEventsError),
            //new("Get all order chains", GetAllOrderChains)
        };
    }

    private bool GetAuditEventsCreate()
    {
        string from = DateTime.UtcNow.AddDays(-2).Date.ToString("o", CultureInfo.InvariantCulture);
        string thru = DateTime.UtcNow.AddDays(-0).Date.ToString("o", CultureInfo.InvariantCulture);

        (var ae, var canCue) = LincaDataExchange.GetAuditEventsCreate(Connection, from, thru);

        if (ae != null)
        {
            Console.WriteLine("Successful create and delete events:");
            BundleHelper.PrintAuditEvents(ae);
        }

        return canCue;
    }

    private bool GetAuditEventsGetPrescr()
    {
        string from = DateTime.UtcNow.AddDays(-100).Date.ToString("o", CultureInfo.InvariantCulture);
        string thru = DateTime.UtcNow.AddDays(-0).Date.ToString("o", CultureInfo.InvariantCulture);

        (var ae, var canCue) = LincaDataExchange.GetAuditEventsGetPrescr(Connection, from, thru);

        if (ae != null)
        {
            Console.WriteLine("Successful GET prescription/s requests:");
            BundleHelper.PrintAuditEvents(ae);
        }

        return canCue;
    }

    private bool GetAuditEventsError()
    {
        string from = DateTime.UtcNow.AddDays(-100).Date.ToString("o", CultureInfo.InvariantCulture);
        string thru = DateTime.UtcNow.AddDays(-0).ToString("o", CultureInfo.InvariantCulture);
        (var ae, var canCue) = LincaDataExchange.GetAuditEventsError(Connection, from, thru);

        if (ae != null)
        {
            Console.WriteLine("Audit events documenting errors:");
            BundleHelper.PrintAuditEvents(ae);
        }

        return canCue;
    }

    private bool GetAllOrderChains()
    {
        string from = DateTime.UtcNow.AddDays(-100).ToString("o", CultureInfo.InvariantCulture);
        (var result, var canCue) = LincaDataExchange.GetAllOrderChains(Connection, from);

        if (result != null)
        {
            BundleHelper.PrintOrderSummary(result);
        }

        return canCue;
    }
}
