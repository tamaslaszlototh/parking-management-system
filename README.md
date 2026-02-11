# Parking Management System



## Overview

- **Domain:** Employee Parking Reservations
- **Purpose:** Enable employees to reserve available parking spots within company-defined allocation rules.

## Actors
- **Parking administrator:** Manages parking spot inventory and system configuration
- **Employee:** Regular staff member who can reserve available parking spots
- **Business manager:** Senior staff with a dedicated parking spot and special privileges



## Use Cases (In progress)

### Common
- **Register User**
- **Login User**

### Parking administrator
- **Add Parking Spot**
  - Business Rules
    - R15. Parking administrator can add new parking spots to the parking spot inventory.
- **Assign Dedicated Parking Spot**
  - Business Rules
    - R11a. Dedicated parking spot reservation is managed by parking administrator.
    - R17. Parking administrator can assign dedicated spots to business manager.

### Employee
- **Reserve Parking Spot**
  - Business Rules
    - R1. Employee can only reserve parking spots for full calendar days (12:00 AM - 11:59 PM).
    - R1a. Employee can reserve future non-overlapping reservations while current reservations are active.
    - R2. Employee can reserve parking spots within a 14-day booking window (from today up to 14 days in the future).
    - R3. Employee can reserve parking spots for up to 14 consecutive days maximum per reservation.
    - R4. Employee can have only one active reservation on a day.
    - R9. System auto-assigns available parking spot for the reservation.
    - R10. System prevents reservation dates that overlap with employee's existing reservations.
- **Cancel Reservation**
  - Business Rules
    - R5. Employee can cancel entire reservation at any time.
    - R6. Employee can cancel individual days from a multi-day reservation.
    - R7. Cancellation releases parking spot immediately for other employees to reserve.

### Business manager
- **Assign Dedicated Parking Spot**
  - Business Rules
    - R11. Business manager is assigned a dedicated parking spot that is automatically reserved for all days of the current calendar year (Jan 1 - Dec 31).
    - R11c. If a business manager is assigned mid-year, their dedicated parking spot is automatically reserved from assignment date through December 31.


## Invariants (In progress)

### Employee
- INV-EMP-1: Single Active Reservation Per Day
- INV-EMP-2: No Overlapping Reservation Dates
- INV-EMP-3: Booking Window Constraint For 14 Days Maximum
- INV-EMP-4: Maximum 14-day Reservation Duration

### Parking Spot
- INV-SPOT-1: Single Occupancy Per Day
- INV-SPOT-2: Valid Parking Spot States
  - ACTIVE: Available for reservations
  - DEACTIVATED: Not available for new reservations
  - DEDICATED: Assigned to a specific business manager
- INV-SPOT-3: Dedicated Parking Spot Uniqueness

### Reservation
- INV-RES-1: Valid Date Range
  - All dates are full calendar days (12:00 AM - 11:59 PM)
  - Maximum 14 days
- INV-RES-2: Assigned Parking Spot Validity
  - At reservation time, the parking spot must be in ACTIVE state (or DEDICATED if business manager cancelled for that day).
- INV-RES-9: Immediate Availability Upon Cancellation
 
### System
- INV-SYS-1: Parking Spot Availability Calculation
  - A parking spot is "available" for date D if and only if:
    - Parking Spot State = ACTIVE or DEDICATED
    - AND NOT EXISTS active reservation for that spot on date D

### Business Manager
- INV-MGR-1: Dedicated Spot Assignment
- INV-MGR-3: Mid-Year Assignment Reservation