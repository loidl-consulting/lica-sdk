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
using Hl7.Fhir.Model.Extensions;
using Lc.Linca.Sdk.Client;

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class US026_KreuzotterPrescriptionTemplate : Spec
{
    public const string UserStory = @"
        Practitioner Dr. Kunibert Kreuzotter is responsible for the LINCA registered mobile caregiver client Flora Hitzebauer. 
        He has received a LINCA order position requesting medication prescription for her.
        He decides to issue a prescription for the medication for Flora Hitzebauer intended by that order position. 
        Hence, he submits a (private) prescription for that position with he got
          and her software will send that to the LINCA server.";

    protected MedicationRequest prescription1 = new();
    // protected MedicationRequest prescription2 = new();

    public US026_KreuzotterPrescriptionTemplate(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
            {
            new("Create PrescriptionMedicationRequest as ordered", CreatePrescriptionRecord)
            };
    }

    private bool CreatePrescriptionRecord()
    {
        (Bundle orders, bool received) = LincaDataExchange.GetProposalsToPrescribe(Connection);

        if (received)
        {
            List<MedicationRequest> proposalsToPrescribe = BundleHelper.FilterProposalsToPrescribe(orders);

            MedicationRequest? orderProposal1 = proposalsToPrescribe.Find(x => x.Id.Equals("   "));  // ENTER ID STRING HERE
            // MedicationRequest? orderProposal2 = proposalsToPrescribe.Find(x => x.Id.Equals("   "));  // ENTER ID STRING HERE

            //if (orderProposal1 == null  || orderProposal2 == null )
            if (orderProposal1 == null)
            {
                Console.WriteLine($"Linca ProposalMedicationRequest for Flora Hitzebauer not found, or it was already processed, prescription cannot be created");

                return false;
            }

            /* PRESCRIPTION 1 */
            prescription1.BasedOn.Add(new()
            {
                Reference = $"LINCAProposalMedicationRequest/{orderProposal1.Id}"
            });

            prescription1.Status = MedicationRequest.MedicationrequestStatus.Active;    // REQUIRED
            prescription1.Intent = MedicationRequest.MedicationRequestIntent.Order;     // REQUIRED
            prescription1.Subject = orderProposal1!.Subject;
            prescription1.Medication = orderProposal1!.Medication;

            prescription1.DosageInstruction = orderProposal1!.DosageInstruction;
            /*
            prescription1.DosageInstruction.Add(new Dosage()
            {
                Text = "1-0-0-0",       // Enter the correct dosage instruction here
            });
            */

            // prescription1.InformationSource will be copied from resource in basedOn by the Fhir server
            // prescription1.Requester will be copied from resource in basedOn by the Fhir server
            // prescription1.SupportingInformation will be copied from resource in basedOn by the Fhir server

            prescription1.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.2",  // OID of designated practitioner 
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                },
                Display = "Dr. Kunibert Kreuzotter"   // optional
            });

            prescription1.DispenseRequest = new() { Quantity = new() { Value = 1 } };
            // prescription1.DispenseRequest = orderProposal1!.DispenseRequest;

            /* PRESCRIPTION 2 */
            /*
            prescription2.BasedOn.Add(new()
            {
                Reference = $"LINCAProposalMedicationRequest/{orderProposal2.Id}"
            });

            prescription2.Status = MedicationRequest.MedicationrequestStatus.Active;    // REQUIRED
            prescription2.Intent = MedicationRequest.MedicationRequestIntent.Order;     // REQUIRED
            prescription2.Subject = orderProposal2!.Subject;
            prescription2.Medication = orderProposal2!.Medication;

            prescription2.DosageInstruction = orderProposal2!.DosageInstruction;
            */

            /*
            prescription2.DosageInstruction.Add(new Dosage()
            {
                Text = "1-0-0-0",       // Enter the correct dosage instruction here
            });
            */

            // prescription2.InformationSource will be copied from resource in basedOn by the Fhir server
            // prescription2.Requester will be copied from resource in basedOn by the Fhir server
            // prescription2.SupportingInformation will be copied from resource in basedOn by the Fhir server

            /*
            prescription2.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.2",  // OID of designated practitioner 
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                },
                Display = "Dr. Kunibert Kreuzotter"   // optional
            });
            */
            // prescription2.DispenseRequest = new() { Quantity = new() { Value = 1 } };
            // prescription2.DispenseRequest = orderProposal2!.DispenseRequest;



            Bundle prescriptions = new()
            {
                Type = Bundle.BundleType.Transaction,
                Entry = new()
            };

            prescriptions.AddResourceEntry(prescription1, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");
            //prescriptions.AddResourceEntry(prescription2, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

            (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

            if (canCue)
            {
                Console.WriteLine($"Linca PrescriptionMedicationRequestBundle transmitted, created Linca PrescriptionMedicationRequests");

                BundleHelper.ShowOrderChains(results);
            }
            else
            {
                Console.WriteLine($"Failed to transmit Linca PrescriptionMedicationRequestBundle");
            }

            if (outcome != null)
            {
                foreach (var item in outcome.Issue)
                {
                    Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                }
            }

            return canCue;
        }
        else
        {
            Console.WriteLine($"Failed to receive ProposalMedicationRequests");

            return false;
        }
    }
}
