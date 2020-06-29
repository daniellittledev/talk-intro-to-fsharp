module File1

type Contact =
    {
        primaryContactDetails
        secondaryContactDetails
    }

// Property manager invites prospective tenant
// 

// Sales, Product

// Sales funnel
// Potential Leads (Everyone)
// Targeted Leads (They could use our service)
// Qualified Leads ()
// 


// Prospects
// Unqualified Lead - in the system
// Qualified Lead - 
// Verified Lead - we can service this person
// Customer - signed up

// Internet Provider Leads
type SalesLead =
    | Prospect
    | UnqualifiedLead // (If we're notified about a lead through a partner)
    | QualifiedLead // (The provider can service this lead, we can connect them)
    | VerifiedLead // They've started an application with our provider
    | Customer // They've been connected



// Prospects (People who could use an internet service)
// 
