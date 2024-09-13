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

namespace Lc.Linca.Sdk;

/// <summary>
/// Methods to interact with the Linked Care FHIR Server
/// </summary>
public static class LincaDataExchange
{
    /// <summary>
    /// Deprecated, for backward compatibility
    /// </summary>
    public static (Patient created, bool canCue) CreatePatient(LincaConnection connection, Patient resource)
    {
        (var created, var cancue, _) = CreatePatientWithOutcome(connection, resource);

        return (created, cancue);
    }

    /// <summary>
    /// Create a new patient record on the FHIR server,
    /// and return the Id that has been assigned.
    /// If the Id is included in the submitted resource,
    /// and a patient with this Id does not yet exist in
    /// the patient store, then the FHIR server will create
    /// the patient resource using the specified Id (external assignment).
    /// If a patient record matching that Id is found, it
    /// it will be updated.
    /// </summary>
    public static (Patient created, bool canCue, OperationOutcome? outcome) CreatePatientWithOutcome(LincaConnection connection, Patient patient)
    {
        (var createdPatient, var canCue, var outcome) = FhirDataExchange<Patient>.CreateResourceWithOutcome(connection, patient);

        if(canCue)
        {
            return (createdPatient, true, outcome);
        }

        return (new(), false, outcome);
    }

    /// <summary>
    /// Deprecated, for backward compatibility
    /// </summary>
    public static (RequestOrchestration createdRO, bool canCue) CreateRequestOrchestration(LincaConnection connection, RequestOrchestration ro)
    {
        (var created, var cancue, _) = CreateRequestOrchestrationWithOutcome(connection, ro);

        return (created, cancue);
    }

    /// <summary>
    /// Post a new Linked Care Medication Order
    /// </summary>
    public static (RequestOrchestration createdRO, bool canCue, OperationOutcome? outcome) CreateRequestOrchestrationWithOutcome(LincaConnection connection, RequestOrchestration ro)
    {
        (var createdRO, var canCue, var outcome) = FhirDataExchange<RequestOrchestration>.CreateResourceWithOutcome(connection, ro);

        if (canCue)
        {
            return (createdRO, true, outcome);
        }

        return (new(), false, outcome);
    }

    /// <summary>
    /// Post a new Linked Care proposal order position
    /// </summary>
    public static (MedicationRequest postedOMR, bool canCue, OperationOutcome? outcome) PostProposalMedicationRequest(LincaConnection connection, MedicationRequest omr)
    {
        (var postedOMR, var canCue, var outcome) = FhirDataExchange<MedicationRequest>.CreateResourceWithOutcome(connection, omr, LincaEndpoints.LINCAProposalMedicationRequest);

        if (canCue)
        {
            return (postedOMR, true, outcome);
        }

        return (new(), false, outcome);
    }

    /// <summary>
    /// Create a LINCAPrescriptionsMedicationRequest: used to stop or end single order positions
    /// </summary>
    public static (MedicationRequest postedPMR, bool canCue, OperationOutcome? outcome) CreatePrescriptionMedicationRequest(LincaConnection connection, MedicationRequest pmr)
    {
        (var postedPMR, var canCue, var outcome) = FhirDataExchange<MedicationRequest>.CreateResourceWithOutcome(connection, pmr, LincaEndpoints.LINCAPrescriptionMedicationRequest);

        if (canCue)
        {
            return (postedPMR, true, outcome);
        }

        return (new(), false, outcome);
    }

    /// <summary>
    /// Create one or more new LINCA prescriptions in one transaction Bundle
    /// all prescriptions in the Bundle must share the same eRezept-Id
    /// </summary>
    public static (Bundle results, bool canCue, OperationOutcome? outcome) CreatePrescriptionBundle(LincaConnection connection, Bundle prescriptions)
    {
        (Bundle createdPrescriptions, bool canCue, var outcome) = FhirDataExchange<Bundle>.CreateResourceBundle(connection, prescriptions, LincaEndpoints.prescription);

        if (canCue)
        {
            return (createdPrescriptions, true, outcome);
        }

        return (new(), false, outcome);
    }

    /// <summary>
    /// Create a new Linked Care medication dispense
    /// </summary>
    public static (MedicationDispense postedMD, bool canCue, OperationOutcome? outcome) CreateMedicationDispense(LincaConnection connection, MedicationDispense md)
    {
        (var postedMD, var canCue, var outcome) = FhirDataExchange<MedicationDispense>.CreateResourceWithOutcome(connection, md);

        if (canCue)
        {
            return (postedMD, true, outcome);
        }

        return (new(), false, outcome);
    }

    /// <summary>
    /// Revoke a Linked Care request orchestration and cancel all contained order positions
    /// </summary>
    public static (OperationOutcome oo, bool deleted) DeleteRequestOrchestration(LincaConnection connection, string id)
    {
        return FhirDataExchange<RequestOrchestration>.DeleteResource(connection, id, LincaEndpoints.LINCARequestOrchestration);
    }

    /// <summary>
    /// Get a all order chain links (proposal order positions, prescriptions, and dispenses) for the given lc_id
    /// </summary>
    public static (Bundle results, bool canCue) GetProposalStatus(LincaConnection connection, string id)
    {
        string operationQuery = $"{LincaEndpoints.proposal_status}?lc_id={id}";
        (Bundle proposalChains, bool canCue) = FhirDataExchange<Bundle>.GetResource(connection, operationQuery);

        if (canCue)
        {
            return (proposalChains, true);
        }

        return (new(), false);
    }

    /// <summary>
    /// Get a all order chain links starting within the last 90 days for the requesting doctor (OID in certificate)
    /// </summary>
    public static (Bundle results, bool canCue) GetProposalsToPrescribe(LincaConnection connection)
    {
        (Bundle proposalChains, bool canCue) = FhirDataExchange<Bundle>.GetResource(connection, LincaEndpoints.proposals_to_prescribe);

        if (canCue)
        {
            return (proposalChains, true);
        }

        return (new(), false);
    }

    /// <summary>
    /// Get all order chain links starting within the last 90 days for the requesting pharmacy (OID in certificate)
    /// </summary>
    public static (Bundle results, bool canCue) GetPrescriptionsToDispense(LincaConnection connection)
    {
        (Bundle proposalChains, bool canCue) = FhirDataExchange<Bundle>.GetResource(connection, LincaEndpoints.prescriptions_to_dispense);

        if (canCue)
        {
            return (proposalChains, true);
        }

        return (new(), false);
    }

    /// <summary>
    /// Get all Linked Care prescriptions which are connected to the given id (eRezept-Id or LinkedCare-prescriptionId)
    /// </summary>
    public static (Bundle results, bool canCue) GetPrescriptionToDispense(LincaConnection connection, string id)
    {
        string operationQuery = $"{LincaEndpoints.prescription_to_dispense}?id={id}";
        (Bundle proposalChains, bool canCue) = FhirDataExchange<Bundle>.GetResource(connection, operationQuery);

        if (canCue)
        {
            return (proposalChains, true);
        }

        return (new(), false);
    }

    /// <summary>
    /// Get initial prescriptions (and corresponding dispenses) from the last 90 days for the given social insurance number (SVNr in Austria)
    /// </summary>
    public static (Bundle results, bool canCue) GetInitialPrescription(LincaConnection connection, string svnr)
    {
        string operationQuery = $"{LincaEndpoints.patient_initial_prescriptions}?svnr={svnr}";
        (Bundle proposalChains, bool canCue) = FhirDataExchange<Bundle>.GetResource(connection, operationQuery);

        if (canCue)
        {
            return (proposalChains, true);
        }

        return (new(), false);
    }

    /// <summary>
    /// Get audit events for successful create and delete requests
    /// </summary>
    public static (Bundle results, bool canCue) GetAuditEventsCreate(LincaConnection connection, string? from, string? thru)
    {
        string operationQuery = CreateOperationQuery(LincaEndpoints.audit_events_create, from, thru);
        (Bundle auditEvents, bool canCue) = FhirDataExchange<Bundle>.GetResource(connection, operationQuery);

        if (canCue)
        {
            return (auditEvents, true);
        }

        return (new(), false);
    }

    /// <summary>
    /// Get audit events for successful create and delete requests
    /// </summary>
    public static (Bundle results, bool canCue) GetAuditEventsError(LincaConnection connection, string? from, string? thru)
    {
        string operationQuery = CreateOperationQuery(LincaEndpoints.audit_events_error, from, thru);
        (Bundle auditEvents, bool canCue) = FhirDataExchange<Bundle>.GetResource(connection, operationQuery);

        if (canCue)
        {
            return (auditEvents, true);
        }

        return (new(), false);
    }

    private static string CreateOperationQuery(string operation, string? from, string? thru)
    {
        string operationQuery = $"{operation}";

        if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(thru))
        {
            operationQuery += $"?from={from}&thru={thru}";
        }
        else if (!string.IsNullOrEmpty(from))
        {
            operationQuery += $"?from={from}";
        }
        else if (!string.IsNullOrEmpty(thru))
        {
            operationQuery += $"?thru={thru}";
        }

        return operationQuery ;
    }

    /// <summary>
    /// This is for testing purposes only
    /// </summary>
    public static (Patient created, bool canCue, OperationOutcome? outcome) PostPatientToProposalMedicationRequest(LincaConnection connection, Patient patient)
    {
        (var createdResource, var canCue, var outcome) = FhirDataExchange<Patient>.CreateResourceWithOutcomeNotTypeSafe(connection, patient, LincaEndpoints.LINCAProposalMedicationRequest);

        if (canCue)
        {
            return (createdResource, true, outcome);
        }

        return (new(), false, outcome);
    }

    /// <summary>
    /// Cancellation of a Linked Care Medication Dispense:
    /// sets the status of the dispense to 'entered-in-error'
    /// </summary>
    public static (OperationOutcome oo, bool deleted) DeleteMedicationDispense(LincaConnection connection, string id)
    {
        return FhirDataExchange<MedicationDispense>.DeleteResource(connection, id, LincaEndpoints.LINCAMedicationDispense);
    }

    /// <summary>
    /// Get a all order chain links (proposal order positions, prescriptions, and dispenses) for the given lc_id
    /// </summary>
    public static (RequestOrchestration created, bool canCue, OperationOutcome? outcome) PostRequestOrchestrationToPatientEndpoint(LincaConnection connection, RequestOrchestration ro)
    {
        (var createdResource, var canCue, var outcome) = FhirDataExchange<RequestOrchestration>.CreateResourceWithOutcomeNotTypeSafe(connection, ro, LincaEndpoints.HL7ATCorePatient);

        if (canCue)
        {
            return (createdResource, true, outcome);
        }

        return (new(), false, outcome);
    }

    /// <summary>
    /// This is for testing purposes only
    /// </summary>
    public static (MedicationRequest created, bool canCue, OperationOutcome? outcome) PostProposalToOrchestrationEndpoint(LincaConnection connection, MedicationRequest mr)
    {
        (var createdResource, var canCue, var outcome) = FhirDataExchange<MedicationRequest>.CreateResourceWithOutcomeNotTypeSafe(connection, mr, LincaEndpoints.LINCARequestOrchestration);

        if (canCue)
        {
            return (createdResource, true, outcome);
        }

        return (new(), false, outcome);
    }

    /// <summary>
    /// This is for testing purposes only
    /// </summary>
    public static (MedicationDispense created, bool canCue, OperationOutcome? outcome) PostDispenseToProposalEndpoint(LincaConnection connection, MedicationDispense dispense)
    {
        (var createdResource, var canCue, var outcome) = FhirDataExchange<MedicationDispense>.CreateResourceWithOutcomeNotTypeSafe(connection, dispense, LincaEndpoints.LINCAProposalMedicationRequest);

        if (canCue)
        {
            return (createdResource, true, outcome);
        }

        return (new(), false, outcome);
    }

    /// <summary>
    /// This is for testing purposes only
    /// </summary>
    public static (Patient created, bool canCue, OperationOutcome? outcome) PostPatientToDispenseEndpoint(LincaConnection connection, Patient patient)
    {
        (var createdResource, var canCue, var outcome) = FhirDataExchange<Patient>.CreateResourceWithOutcomeNotTypeSafe(connection, patient, LincaEndpoints.LINCAMedicationDispense);

        if (canCue)
        {
            return (createdResource, true, outcome);
        }

        return (new(), false, outcome);
    }

    /// <summary>
    /// This is for testing purposes only
    /// </summary>
    public static (Bundle results, bool canCue) GetWithAnyOperationName(LincaConnection connection, string testOperationName)
    {
        (Bundle proposalChains, bool canCue) = FhirDataExchange<Bundle>.GetResource(connection, testOperationName);

        if (canCue)
        {
            return (proposalChains, true);
        }

        return (new(), false);
    }

    /// <summary>
    /// This is for testing purposes only
    /// </summary>
    public static (Bundle results, bool canCue, OperationOutcome? outcome) PostToAnyOperationOrResourceName(LincaConnection connection, Bundle prescriptions, string postTo)
    {
        (Bundle createdPrescriptions, bool canCue, var outcome) = FhirDataExchange<Bundle>.CreateResourceBundle(connection, prescriptions, postTo);

        if (canCue)
        {
            return (createdPrescriptions, true, outcome);
        }

        return (new(), false, outcome);
    }
}
