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
using Lc.Linca.Sdk.Client;

namespace Lc.Linca.Sdk.Specs.ActorPharmacy;

internal class US019_DispenseLasix : Spec
{
    

    public const string UserStory = @"
        Pharmacist Mag. Franziska Fröschl, owner of the pharmacy Apotheke 'Klappernder Storch' has 
        access to and permission in a pharmacist role in the LINCA system. 
        When she is expected to fullfil medication orders for a customer, e.g., Gertrude Steinmaier, 
        and she has a LINCA order Id to go with a purchase her care giver just made for her, 
        then Mag. Fröschl submits a dispense record for the order position in question
          and her software will send that to the LINCA server.";

    public US019_DispenseLasix(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Create MedicationDispense", CreateMedicationDispenseRecord)
        };

    }

    private bool CreateMedicationDispenseRecord()
    {
        //LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (Bundle orders, bool received) = LincaDataExchange.GetPrescriptionToDispense(Connection, " ");

        if (received)
        {
            List<MedicationRequest> prescriptionsToDispense = BundleHelper.FilterPrescriptionsToDispense(orders);

            // MedicationRequest? prescriptionLasix = prescriptionsToDispense.Find(x => x.Medication.Concept.Coding.First().Code.Equals("0031130"));

            if (prescriptionsToDispense.Count != 5)
            {
                Console.WriteLine("Linca PrescriptionMedicationRequests not correct, LINCAMedicationDispense cannot be created");

                return (false);
            }

            var dispenseCount = 0;

            foreach (var prescription in prescriptionsToDispense)
            {
                MedicationDispense dispense1 = new();

                dispense1.AuthorizingPrescription.Add(new()
                {
                    Reference = $"LINCAPrescriptionMedicationRequest/{prescription.Id}"
                });

                dispense1.Status = MedicationDispense.MedicationDispenseStatusCodes.Completed;
                dispense1.Subject = prescription.Subject;

                dispense1.Medication = prescription.Medication;

                dispense1.Quantity = prescription.DispenseRequest.Quantity;

                dispense1.DosageInstruction = prescription.DosageInstruction;


                dispense1.Performer.Add(new()
                {
                    Actor = new()
                    {
                        Identifier = new()
                        {
                            Value = "1.2.40.0.34.3.1.11173",  // OID of dispensing pharmacy
                            System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                        }
                    }
                });

                dispense1.Type = new()
                {
                    Coding = new()
                {
                    new Coding(system: "http://terminology.hl7.org/CodeSystem/v3-ActCode", code: "FFC") // complete the dispense
                }
                };

                (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense1);

                if (canCue)
                {
                    Console.WriteLine($"Linca MedicationDispense transmitted, id {postedMD.Id}");
                    dispenseCount++;
                }
                else
                {
                    Console.WriteLine($"Failed to transmit Linca MedicationDispense");
                }

                if (outcome != null)
                {
                    foreach (var item in outcome.Issue)
                    {
                        Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                    }
                }
            }

            return dispenseCount == 5;
        }
        else
        {
            Console.WriteLine($"Failed to receive Linca Prescription Medication Requests");

            return false;
        }
    }
}
