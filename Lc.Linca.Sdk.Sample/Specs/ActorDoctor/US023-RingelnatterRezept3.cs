﻿/***********************************************************************************
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

internal class US023_RingelnatterRezept3 : Spec
{
    public const string UserStory = @"
        Practitioner Dr. Kunibert Kreuzotter is responsible for the LINCA registered mobile caregiver client Gertrude Steinmaier. 
        He has received a LINCA order position requesting medication prescription for her.
        He decides to issue a prescription for the medication for Gertrude Steinmaier intended by that order position. 
        Hence, he submits a prescription for that position with the eMedId and eRezeptId he got
          and her software will send that to the LINCA server.";

    protected MedicationRequest prescription1 = new();
    protected MedicationRequest prescription2 = new();

    public US023_RingelnatterRezept3(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
            {
            new("Create PrescriptionMedicationRequests as ordered", CreatePrescriptionRecord)
            };
    }

    private bool CreatePrescriptionRecord()
    {
        (Bundle orders, bool received) = LincaDataExchange.GetProposalsToPrescribe(Connection);

        if (received)
        {
            List<MedicationRequest> proposalsToPrescribe = BundleHelper.FilterProposalsToPrescribe(orders);

            MedicationRequest? orderProposalCalci = proposalsToPrescribe.Find(x => x.Id.Equals("18af4693f0d04b9e964e6e347d3e01e2"));  // ENTER ID STRING HERE
            MedicationRequest? orderProposalAtor = proposalsToPrescribe.Find(x => x.Id.Equals("c2f8e1bee0824dd7be0a231ae4f75854"));  // ENTER ID STRING HERE

            if (orderProposalCalci == null || orderProposalAtor == null )
            {
                Console.WriteLine($"Linca ProposalMedicationRequest for Gertrude Steinmaier not found, or it was already processed, prescription cannot be created");

                return false;
            }

            /***********************************************/
            prescription1.BasedOn.Add(new()
            {
                Reference = $"LINCAProposalMedicationRequest/{orderProposalCalci.Id}"
            });

            prescription1.Status = MedicationRequest.MedicationrequestStatus.Active;    // REQUIRED
            prescription1.Intent = MedicationRequest.MedicationRequestIntent.Order;     // REQUIRED
            prescription1.Subject = orderProposalCalci!.Subject;
            prescription1.Medication = orderProposalCalci!.Medication;

            prescription1.DosageInstruction.Add(new Dosage()
            {
                Text = "1-0-0-0",
            });

            // prescription.InformationSource will be copied from resource in basedOn by the Fhir server
            // prescription.Requester will be copied from resource in basedOn by the Fhir server
            // prescription.SupportingInformation will be copied from resource in basedOn by the Fhir server

            prescription1.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                }
            });

            prescription1.GroupIdentifier = new()
            {
                Value = "97UHBD7EJB8L",
                System = "urn:oid:1.2.40.0.10.1.4.3.3"       // OID: Rezeptnummer
            };

            prescription1.DispenseRequest = new() { Quantity = new() { Value = 2 } };

            /*******************************************************************/

            /********************************************************************/
            prescription2.BasedOn.Add(new()
            {
                Reference = $"LINCAProposalMedicationRequest/{orderProposalAtor.Id}"
            });

            prescription2.Status = MedicationRequest.MedicationrequestStatus.Active;    // REQUIRED
            prescription2.Intent = MedicationRequest.MedicationRequestIntent.Order;     // REQUIRED
            prescription2.Subject = orderProposalAtor!.Subject;
            prescription2.Medication = orderProposalAtor!.Medication;

            prescription2.DosageInstruction.Add(new Dosage()
            {
                Text = "0-0-1-0",
            });

            // prescription.InformationSource will be copied from resource in basedOn by the Fhir server
            // prescription.Requester will be copied from resource in basedOn by the Fhir server
            // prescription.SupportingInformation will be copied from resource in basedOn by the Fhir server

            prescription2.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                }
            });

            prescription2.GroupIdentifier = new()
            {
                Value = "97UHBD7EJB8L",
                System = "urn:oid:1.2.40.0.10.1.4.3.3"       // OID: Rezeptnummer
            };

            prescription2.DispenseRequest = new() { Quantity = new() { Value = 2 } };


            /******************************************/
            Bundle prescriptions = new()
            {
                Type = Bundle.BundleType.Transaction,
                Entry = new()
            };


            prescriptions.AddResourceEntry(prescription1, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");
            prescriptions.AddResourceEntry(prescription2, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

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
