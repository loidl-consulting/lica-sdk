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
using Lc.Linca.Sdk.Client;
using System.Globalization;

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class US022_MedOrderRingelnatter : Spec
{
    public const string UserStory = @"
        User Walter Specht (DGKP) is a caregiver in the inpatient care facility Haus Vogelsang. 
        He needs to collectively order prescription medication for several clients, amongst others 
        for Günter Gürtelthier and Patrizia Platypus. Patrizia's practitioner is 
        Dr. Kunibert Kreuzotter, Günter's practitioner is Dr. Silvia Spitzmaus. 
        Walter Specht places an order for all needed client prescription medication on LINCA 
        and specifies in advance the pharmacy Apotheke 'Zum frühen Vogel' that ought 
        to prepare the order";

    protected Patient createdRicarda = new();
    protected MedicationRequest medReq1 = new();
    protected MedicationRequest medReq2 = new();
    protected MedicationRequest medReq3 = new();
    protected MedicationRequest medReq4 = new();
    protected MedicationRequest medReq5 = new();
    protected MedicationRequest medReq6 = new();
    protected MedicationRequest medReq7 = new();
    protected MedicationRequest medReq8 = new();


    public US022_MedOrderRingelnatter(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
        {
            new("Create client record Ricarda Ringelnatter", CreateClientRecord),
            new("Place 3 orders for Ricarda Ringelnatter", CreateRequestOrchestrationRecord1),
            new("Place 3 more orders for Ricarda Ringelnatter", CreateRequestOrchestrationRecord2),
            new("Place 2 more orders for Ricarda Ringelnatter", CreateRequestOrchestrationRecord3)
        };
    }

    private bool CreateClientRecord()
    {
        var patient = new Patient();

        patient.Name.Add(new()
        {
            Family = "Ringelnatter TEST",
            Given = new[] { "Ricarda" },
            Text = "Ricarda Ringelnatter TEST"
        });

        patient.BirthDate = DateTime.ParseExact(
            "20050219",
            Constants.DobFormat,
            CultureInfo.InvariantCulture
        ).ToFhirDate();

        patient.Identifier.Add(new Identifier(
            system: Constants.WellknownOidSocialInsuranceNr,
            value: "7159101374"
        ));

        patient.Gender = AdministrativeGender.Female;

        (createdRicarda, var canCue, var outcome) = LincaDataExchange.CreatePatientWithOutcome(Connection, patient);

        if (canCue)
        {
            //LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdPatrizia = createdPatrizia.Id;
            //LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();

            Console.WriteLine($"Client information transmitted, id {createdRicarda.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit client information");
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

    private bool CreateRequestOrchestrationRecord1() 
    {  
        PrepareMedicationRequests1();

        RequestOrchestration ro = new()
        {
            Status = RequestStatus.Active,      // REQUIRED
            Intent = RequestIntent.Proposal,    // REQUIRED
            Subject = new ResourceReference()   // REQUIRED
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization from certificate
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                },
                Display = "Haus Vogelsang"   // optional
            }
        };

        ro.Contained.Add(medReq1);
        ro.Contained.Add(medReq2);
        ro.Contained.Add(medReq3);  

        foreach (var item in ro.Contained)
        {
            var action = new RequestOrchestration.ActionComponent()
            {
                //Type =
                Resource = new ResourceReference($"#{item.Id}")
            };
            ro.Action.Add(action);
        }

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            //LinkedCareSampleClient.CareInformationSystemScaffold.Data.LcIdVogelsang = createdRO.Id;
            //LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();

            Console.WriteLine($"Linca Request Orchestration transmitted, id {createdRO.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit Linca Request Orchestration");
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

    private void PrepareMedicationRequests1()
    {
        //LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();
        
        // medication request 1 for Ricarda
        medReq1.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED
        medReq1.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq1.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq1.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{createdRicarda.Id}"     // relative path to Linca Fhir patient resource
        };

        medReq1.Medication = new()
        {
            Concept = new()
            {
                Coding = new()
                {
                    new Coding()
                    {
                        Code = "0533245",
                        System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                        Display = "MARCOUMAR TBL 3MG"
                    }
                }
            }
        };

        medReq1.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Haus Vogelsang"   // optional
        });

        medReq1.Requester = new ResourceReference()  // REQUIRED
        {
            Identifier = new()
            {
                Value = "ECHT_SPECHT",               // e.g., org internal username or handsign of Susanne Allzeit
                System = "urn:oid:2.999.40.0.34.1.1.1"  // Code-System: Care-Org Pflegedienst Immerdar
            },
            Display = "DGKP Walter Specht"
        };

        medReq1.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            }
            //Display = "Dr. Silvia Spitzmaus"   // optional
        });

        medReq1.DispenseRequest = new()
        {
            Quantity = new() { Value = 2  }
        };

        // medication request 2 for Ricarda
        medReq2.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED
        medReq2.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq2.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq2.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{createdRicarda.Id}"     // relative path to Linca Fhir patient resource
        };

        medReq2.Medication = new()
        {
            Concept = new()
            {
                Coding = new()
                {
                    new Coding()
                    {
                        Code = "3902401",
                        System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                        Display = "CANDESARTAN +PH TBL  8MG"
                    }
                }
            }
        };

        medReq2.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Haus Vogelsang"   // optional
        });

        medReq2.Requester = new ResourceReference()  // REQUIRED
        {
            Identifier = new()
            {
                Value = "ECHT_SPECHT",               // e.g., org internal username or handsign of Susanne Allzeit
                System = "urn:oid:2.999.40.0.34.1.1.1"  // Code-System: Care-Org Pflegedienst Immerdar
            },
            Display = "DGKP Walter Specht"
        };

        medReq2.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            }
            //Display = "Dr. Silvia Spitzmaus"   // optional
        });

        medReq2.DispenseRequest = new()
        {
            Quantity = new() { Value = 1}
        };

        /***********************************************************************************/

        // medication request 3 for Ricarda
        medReq3.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED
        medReq3.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq3.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq3.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{createdRicarda.Id}"     // relative path to Linca Fhir patient resource
        };

        medReq3.Medication = new()
        {
            Concept = new()
            {
                Coding = new()
                {
                    new Coding()
                    {
                        Code = "4466037",
                        System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                        Display = "HUMIRA INJ FSPR 80MG/0,8ML"
                    }
                }
            }
        };

        medReq3.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Haus Vogelsang"   // optional
        });

        medReq3.Requester = new ResourceReference()  // REQUIRED
        {
            Identifier = new()
            {
                Value = "ECHT_SPECHT",               // e.g., org internal username or handsign of Susanne Allzeit
                System = "urn:oid:2.999.40.0.34.1.1.1"  // Code-System: Care-Org Pflegedienst Immerdar
            },
            Display = "DGKP Walter Specht"
        };

        medReq3.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            }
            //Display = "Dr. Silvia Spitzmaus"   // optional
        });

        medReq3.DispenseRequest = new()
        {
            Quantity = new() { Value = 1}
        };
    }

    private bool CreateRequestOrchestrationRecord2()
    {
        PrepareMedicationRequests2();

        RequestOrchestration ro = new()
        {
            Status = RequestStatus.Active,      // REQUIRED
            Intent = RequestIntent.Proposal,    // REQUIRED
            Subject = new ResourceReference()   // REQUIRED
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization from certificate
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                },
                Display = "Haus Vogelsang"   // optional
            }
        };

        ro.Contained.Add(medReq4);
        ro.Contained.Add(medReq5);
        ro.Contained.Add(medReq6);

        foreach (var item in ro.Contained)
        {
            var action = new RequestOrchestration.ActionComponent()
            {
                //Type =
                Resource = new ResourceReference($"#{item.Id}")
            };
            ro.Action.Add(action);
        }

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            //LinkedCareSampleClient.CareInformationSystemScaffold.Data.LcIdVogelsang = createdRO.Id;
            //LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();

            Console.WriteLine($"Linca Request Orchestration transmitted, id {createdRO.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit Linca Request Orchestration");
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

    private void PrepareMedicationRequests2()
    {
        // LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        // medication request 1 for Ricarda
        medReq4.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED
        medReq4.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq4.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq4.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{createdRicarda.Id}"     // relative path to Linca Fhir patient resource
        };

        medReq4.Medication = new()
        {
            Concept = new()
            {
                Coding = new()
                {
                    new Coding()
                    {
                        Code = "3909780",
                        System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                        Display = "MEXALEN TBL 500MG"
                    }
                }
            }
        };

        medReq4.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Haus Vogelsang"   // optional
        });

        medReq4.Requester = new ResourceReference()  // REQUIRED
        {
            Identifier = new()
            {
                Value = "ECHT_SPECHT",               // e.g., org internal username or handsign of Susanne Allzeit
                System = "urn:oid:2.999.40.0.34.1.1.1"  // Code-System: Care-Org Pflegedienst Immerdar
            },
            Display = "DGKP Walter Specht"
        };

        medReq4.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            }
            //Display = "Dr. Silvia Spitzmaus"   // optional
        });

        medReq4.DispenseRequest = new()
        {
            Quantity = new() { Value = 2 }
        };

        // medication request 2 for Ricarda
        medReq5.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED
        medReq5.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq5.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq5.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{createdRicarda.Id}"     // relative path to Linca Fhir patient resource
        };

        medReq5.Medication = new()
        {
            Concept = new()
            {
                Coding = new()
                {
                    new Coding()
                    {
                        Code = "0533363",
                        System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                        Display = "PARKEMED FTBL 500MG"
                    }
                }
            }
        };

        medReq5.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Haus Vogelsang"   // optional
        });

        medReq5.Requester = new ResourceReference()  // REQUIRED
        {
            Identifier = new()
            {
                Value = "ECHT_SPECHT",               // e.g., org internal username or handsign of Susanne Allzeit
                System = "urn:oid:2.999.40.0.34.1.1.1"  // Code-System: Care-Org Pflegedienst Immerdar
            },
            Display = "DGKP Walter Specht"
        };

        medReq5.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            }
            //Display = "Dr. Silvia Spitzmaus"   // optional
        });

        medReq5.DispenseRequest = new()
        {
            Quantity = new() { Value = 1 }
        };

        /***********************************************************************************/

        // medication request 3 for Ricarda
        medReq6.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED
        medReq6.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq6.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq6.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{createdRicarda.Id}"     // relative path to Linca Fhir patient resource
        };

        medReq6.Medication = new()
        {
            Concept = new()
            {
                Coding = new()
                {
                    new Coding()
                    {
                        Code = "1345540",
                        System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                        Display = "FOLSAN TBL 5MG"
                    }
                }
            }
        };

        medReq6.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Haus Vogelsang"   // optional
        });

        medReq6.Requester = new ResourceReference()  // REQUIRED
        {
            Identifier = new()
            {
                Value = "ECHT_SPECHT",               // e.g., org internal username or handsign of Susanne Allzeit
                System = "urn:oid:2.999.40.0.34.1.1.1"  // Code-System: Care-Org Pflegedienst Immerdar
            },
            Display = "DGKP Walter Specht"
        };

        medReq6.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            }
            //Display = "Dr. Silvia Spitzmaus"   // optional
        });

        medReq6.DispenseRequest = new()
        {
            Quantity = new() { Value = 2 }
        };
    }

    private bool CreateRequestOrchestrationRecord3()
    {
        PrepareMedicationRequests3();

        RequestOrchestration ro = new()
        {
            Status = RequestStatus.Active,      // REQUIRED
            Intent = RequestIntent.Proposal,    // REQUIRED
            Subject = new ResourceReference()   // REQUIRED
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization from certificate
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                },
                Display = "Haus Vogelsang"   // optional
            }
        };

        ro.Contained.Add(medReq7);
        ro.Contained.Add(medReq8);

        foreach (var item in ro.Contained)
        {
            var action = new RequestOrchestration.ActionComponent()
            {
                //Type =
                Resource = new ResourceReference($"#{item.Id}")
            };
            ro.Action.Add(action);
        }

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            //LinkedCareSampleClient.CareInformationSystemScaffold.Data.LcIdVogelsang = createdRO.Id;
            //LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();

            Console.WriteLine($"Linca Request Orchestration transmitted, id {createdRO.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit Linca Request Orchestration");
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

    private void PrepareMedicationRequests3()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        // medication request 1 for Ricarda
        medReq7.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED
        medReq7.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq7.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq7.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{createdRicarda.Id}"     // relative path to Linca Fhir patient resource
        };

        medReq7.Medication = new()
        {
            Concept = new()
            {
                Coding = new()
                {
                    new Coding()
                    {
                        Code = "3909892",
                        System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                        Display = "CALCIDURAN FTBL 500MG/800IE"
                    }
                }
            }
        };

        medReq7.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Haus Vogelsang"   // optional
        });

        medReq7.Requester = new ResourceReference()  // REQUIRED
        {
            Identifier = new()
            {
                Value = "ECHT_SPECHT",               // e.g., org internal username or handsign of Susanne Allzeit
                System = "urn:oid:2.999.40.0.34.1.1.1"  // Code-System: Care-Org Pflegedienst Immerdar
            },
            Display = "DGKP Walter Specht"
        };

        medReq7.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            }
            //Display = "Dr. Silvia Spitzmaus"   // optional
        });

        medReq7.DispenseRequest = new()
        {
            Quantity = new() { Value = 2 }
        };

        // medication request 2 for Ricarda
        medReq8.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED
        medReq8.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq8.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq8.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{createdRicarda.Id}"     // relative path to Linca Fhir patient resource
        };

        medReq8.Medication = new()
        {
            Concept = new()
            {
                Coding = new()
                {
                    new Coding()
                    {
                        Code = "3904771",
                        System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                        Display = "ATORVALAN FTBL 10MG"
                    }
                }
            }
        };

        medReq8.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Haus Vogelsang"   // optional
        });

        medReq8.Requester = new ResourceReference()  // REQUIRED
        {
            Identifier = new()
            {
                Value = "ECHT_SPECHT",               // e.g., org internal username or handsign of Susanne Allzeit
                System = "urn:oid:2.999.40.0.34.1.1.1"  // Code-System: Care-Org Pflegedienst Immerdar
            },
            Display = "DGKP Walter Specht"
        };

        medReq8.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            }
            //Display = "Dr. Silvia Spitzmaus"   // optional
        });

        medReq8.DispenseRequest = new()
        {
            Quantity = new() { Value = 2 }
        };
    }
}
