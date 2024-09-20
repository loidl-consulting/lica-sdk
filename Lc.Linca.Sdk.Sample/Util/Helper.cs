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
using Hl7.Fhir.Support;
using Hl7.Fhir.Utility;
using System.Xml.Serialization;

namespace Lc.Linca.Sdk;

/// <summary>
/// Utilities for filtering and printing Bundles
/// </summary>
public static class BundleHelper
{
    public static List<MedicationRequest> FilterProposalsToPrescribe(Bundle orderchains)
    {
        List<MedicationRequest> openProposals = new();
        List<MedicationRequest> proposals = new();
        List<MedicationRequest> prescriptions = new();

        foreach (var item in orderchains.Entry)
        {
            if (item.FullUrl.Contains("LINCAProposal"))
            {
                proposals.Add((item.Resource as MedicationRequest)!);
            }

            if (item.FullUrl.Contains("LINCAPrescription"))
            {
                prescriptions.Add((item.Resource as MedicationRequest)!) ;
            }
        }

        foreach (var item in proposals) 
        {
            if (proposals.Find(x => !x.BasedOn.IsNullOrEmpty() && x.BasedOn.First().Reference.Contains(item.Id)) ==  null
                && prescriptions.Find(x => !x.BasedOn.IsNullOrEmpty() && x.BasedOn.First().Reference.Contains(item.Id)) == null)
            {
                openProposals.Add(item);
            }        
        }

        return openProposals;
    }

    public static List<MedicationRequest> FilterPrescriptionsToDispense(Bundle orderchains)
    {
        List<MedicationRequest> openPrescriptions = new();
        List<MedicationRequest> prescriptions = new();
        List<MedicationDispense> dispenses = new();

        foreach (var item in orderchains.Entry)
        {
            if (item.FullUrl.Contains("LINCAPrescription"))
            {
                prescriptions.Add((item.Resource as MedicationRequest)!);

            }

            if (item.FullUrl.Contains("LINCAMedicationDispense"))
            {
                dispenses.Add((item.Resource as MedicationDispense)!);
            }
        }

        foreach (var item in prescriptions)
        {
            if (prescriptions.Find(x => x.PriorPrescription != null && x.PriorPrescription.Reference.Contains(item.Id)) == null
               && dispenses.Find(x => !x.AuthorizingPrescription.IsNullOrEmpty()
                                        && x.AuthorizingPrescription.First().Reference.Contains(item.Id)
                                        && x.Type.Coding.First().Code.EndsWith("C")
                                        && x.Status == MedicationDispense.MedicationDispenseStatusCodes.Completed) == null)
            {
                openPrescriptions.Add(item);
            }
        }

        return openPrescriptions;
    }

    public static void ShowOrderChains (Bundle orderchains)
    {
        List<MedicationRequest> proposals = new();
        List<MedicationRequest> prescriptions = new();
        List<MedicationDispense> dispenses = new();

        Console.WriteLine("Bundle Entries:");

        foreach (var item in orderchains.Entry)
        {
            Console.WriteLine(item.FullUrl);

            if (item.FullUrl.Contains("LINCAProposal"))
            {
                proposals.Add((item.Resource as MedicationRequest)!);
            }
            else if (item.FullUrl.Contains("LINCAPrescription"))
            {
                prescriptions.Add((item.Resource as MedicationRequest)!);
            }
            else if (item.FullUrl.Contains("LINCAMedicationDispense"))
            {
                dispenses.Add((item.Resource as MedicationDispense)!);
            }
        }

        Console.WriteLine($"Proposals: {proposals.Count}");
        Console.WriteLine($"Prescriptions: {prescriptions.Count}");
        Console.WriteLine($"Prescriptions: {dispenses.Count}");

        foreach (var item in dispenses)
        {
            if (item.AuthorizingPrescription.Count == 1)
            {
                Console.WriteLine($"Dispense Id: {item.Id} for {item.Subject.Display} --> authorizingPrescription: {item.AuthorizingPrescription.First().Reference}");
            }
            else
            {
                Console.WriteLine($"Dispense Id: {item.Id} for {item.Subject.Display} is a dispense without prescription (OTC)");
            }
        }

        foreach (var item in prescriptions)
        {
            if (item.BasedOn.Count == 1)
            {
                Console.WriteLine($"Prescription Id: {item.Id} for {item.Subject.Display} --> is based on proposal: {item.BasedOn.First().Reference}");
            }
            else if (item.PriorPrescription != null) 
            {
                Console.WriteLine($"Prescription Id: {item.Id} for {item.Subject.Display} --> refers to prior prescription: {item.PriorPrescription.Reference}");
            }
            else
            {
                Console.WriteLine($"Prescription Id: {item.Id} for {item.Subject.Display} is an initial prescription");
            }
        }

        foreach (var item in proposals)
        {
            if (item.BasedOn.Count == 1)
            {
                Console.WriteLine($"Proposal Id: {item.Id} for {item.Subject.Display} --> is based on proposal: {item.BasedOn.First().Reference}");
            }
            else
            {
                Console.WriteLine($"Proposal Id: {item.Id} for {item.Subject.Display} --> is a starting link");
            }
        }
    }

    public static void PrintOrderSummary(Bundle orderchains)
    {
        List<MedicationRequest> initialProposals = new();
        List<MedicationRequest> basedOnProposals = new();
        List<MedicationRequest> initialPrescriptions = new();
        List<MedicationRequest> basedOnPrescriptions = new();
        List<MedicationDispense> otcDispenses = new();
        List<MedicationDispense> dispenses = new();

        foreach (var item in orderchains.Entry)
        {
            if (item.FullUrl.Contains("LINCAProposal"))
            {
                var prop = item.Resource as MedicationRequest;

                if (prop!.BasedOn == null || prop.BasedOn.Count == 0)
                {
                    initialProposals.Add(prop);
                }
                else
                {
                    basedOnProposals.Add(prop);
                }
            }
            else if (item.FullUrl.Contains("LINCAPrescription"))
            {
                var presc = item.Resource as MedicationRequest;

                if (presc!.BasedOn == null || presc.BasedOn.Count == 0)
                {
                    initialPrescriptions.Add(presc);
                }
                else
                {
                    basedOnPrescriptions.Add(presc);    
                }
            }
            else if (item.FullUrl.Contains("LINCAMedicationDispense"))
            {
                var disp = item.Resource as MedicationDispense;

                if (disp!.AuthorizingPrescription == null || disp.AuthorizingPrescription.Count == 0)
                {
                    otcDispenses.Add(disp);
                }
                else
                {
                    dispenses.Add(disp);
                }
            }
        }

        Console.WriteLine($"{initialProposals.Count} proposals");
        Console.WriteLine($"{basedOnProposals.Count} changes to proposals");
        Console.WriteLine($"{basedOnPrescriptions.Count} prescriptions to proposals");
        Console.WriteLine($"{initialPrescriptions.Count} initial prescripions");
        Console.WriteLine($"{dispenses.Count} dispenses to prescriptions");
        Console.WriteLine($"{otcDispenses.Count} otc dispenses");

        initialProposals = initialProposals.OrderBy(p => p.Meta.LastUpdated).ToList();

        foreach (var prop in initialProposals)
        {
            Console.WriteLine($"Proposal ID: {prop.Id} Recorded: {prop.Meta.LastUpdated} Patient: {prop.Subject.Identifier?.Value}|{prop.Subject.Display} " +
                $"Medication: {prop.Medication.Concept.Coding.First().Code}|{prop.Medication.Concept.Coding.First().Display} " +
                $"Quantity: {prop.DispenseRequest?.Quantity?.Value} Dosage: {prop.DosageInstruction?.FirstOrDefault()?.Text}");

            var succ = basedOnProposals.FirstOrDefault(p => p.BasedOn.First().Reference.Contains(prop.Id));
            if (succ != null)
            {
                // Proposal was cancelled, updates are not possible in mynevaToGo
                Console.WriteLine($"Proposal ID: {succ.Id} Status: {succ.Status}");
                basedOnProposals.Remove(succ);
            }
            else
            {
                var pres = basedOnPrescriptions.FirstOrDefault(p => p.BasedOn.First().Reference.Contains(prop.Id));
                if (pres != null)
                {
                    Console.WriteLine($"Prescrip ID: {pres.Id} Patient: {pres.Subject.Display} " +
                        $"Medication: {pres.Medication.Concept.Coding.First().Code}|{pres.Medication.Concept.Coding.First().Display} " +
                        $"Quantity: {pres.DispenseRequest.Quantity.Value} Dosage: {pres.DosageInstruction.First().Text}");
                    basedOnPrescriptions.Remove(pres);

                    var disp = dispenses.FindAll(d => d.AuthorizingPrescription.First().Reference == pres.Id);
                    if (disp.Any())
                    {
                        foreach (var d in  disp)
                        {
                            Console.WriteLine($"Dispense ID: {d.Id} Patient: {d.Subject.Display} " +
                                $"Medication: {d.Medication.Concept.Coding.First().Code}|{d.Medication.Concept.Coding.First().Display} " +
                                $"Quantity: {d.Quantity.Value} Dosage: {d.DosageInstruction.First().Text} " +
                                $"Type: {d.Type.Coding.First().Code} Status: {d.Status}");
                        }
                        dispenses.RemoveAll(d => d.AuthorizingPrescription.First().Reference == pres.Id);
                    }
                }
            }
            Console.WriteLine("---------------------------------------------------------------------");
        }

        if (initialPrescriptions.Any())
        {
            Console.WriteLine("ATTENTION INITIAL PRESCRIPTIONS FOUND");
            foreach (var pres in initialPrescriptions)
            {
                Console.WriteLine($"Prescrip ID: {pres.Id} Patient: {pres.Subject.Display} " +
                    $"Medication: {pres.Medication.Concept.Coding.First().Code}|{pres.Medication.Concept.Coding.First().Display} " +
                    $"Quantity: {pres.DispenseRequest.Quantity.Value} Dosage: {pres.DosageInstruction.First().Text}");
            }
            Console.WriteLine("---------------------------------------------------------------------");
        }

        if( otcDispenses.Any() )
        {
            Console.WriteLine("ATTENTION OTC DISPENSES FOUND");
            foreach (var d in otcDispenses)
            {
                Console.WriteLine($"Dispense ID: {d.Id} Patient: {d.Subject.Display} " +
                    $"Medication: {d.Medication.Concept.Coding.First().Code}|{d.Medication.Concept.Coding.First().Display} " +
                    $"Quantity: {d.Quantity.Value} Dosage: {d.DosageInstruction.First().Text} " +
                    $"Type: {d.Type.Coding.First().Code} Status: {d.Status}");
            }
            Console.WriteLine("---------------------------------------------------------------------");
        }

    }

    public static void PrintAuditEvents(Bundle auditEvents)
    {
        List<AuditEvent> ae = new();

        foreach (var item in auditEvents.Entry)
        {
            ae.Add((item.Resource as AuditEvent)!);
        }

        foreach (var item in ae)
        {
            Console.WriteLine($"Event record time: {item.Recorded}" +
                $"\tRequested by: {item.Agent.First().Who.Identifier.Value}" +
                $"\tDetails: {item.Outcome.Detail?.FirstOrDefault()?.Text}");

            if (item.Contained != null && item.Contained.Count > 0)
            {
                foreach (var oo in item.Contained)
                {
                    if (oo is OperationOutcome outcome)
                    {
                        foreach (var issue in outcome.Issue)
                        {
                            Console.WriteLine($"Issue \tSeverity: {issue.Severity?.ToString()} \tErrortype: {issue.Code} \tErrorcode: {issue.Details?.Coding?.FirstOrDefault()?.Code} \t{issue.Details?.Text}");
                        }
                    }
                }
            }
            Console.WriteLine("");
        }
    }
}

public class OutcomeHelper
{
    public static bool PrintOutcomeAndCheckLCVAL(OperationOutcome? outcome, string lcval)
    {
        bool containsLCVAL = false;

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                if (item.Details.Coding?.FirstOrDefault()?.Code.Contains(lcval) ?? false)
                {
                    containsLCVAL = true;
                }
            }
        }

        return containsLCVAL;
    }

    public static void PrintOutcome(OperationOutcome? outcome)
    {
        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }
    }
}
