////////////////////////////////////////////////////////
/*
  This file is part of CNCLib - A library for stepper motors.

  Copyright (c) 2013-2017 Herbert Aitenbichler

  CNCLib is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  CNCLib is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.
  http://www.gnu.org/licenses/
*/
////////////////////////////////////////////////////////

#pragma once

////////////////////////////////////////////////////////

#define STEPPERTYPE 4		// CStepperCNCShield

////////////////////////////////////////////////////////

#define USBBAUDRATE 250000

////////////////////////////////////////////////////////

#define MYNUM_AXIS 2

////////////////////////////////////////////////////////

#define X_MAXSIZE 36000				// in mm1000_t
#define Y_MAXSIZE 36000 
#define Z_MAXSIZE 10000 
#define A_MAXSIZE 50000 
#define B_MAXSIZE 360000 
#define C_MAXSIZE 360000 

////////////////////////////////////////////////////////

#define STEPPERDIRECTION 0		// set bit to invert direction of each axis

#define STEPSPERROTATION	20
#define MICROSTEPPING		16
#define SCREWLEAD			3.0

#define X_STEPSPERMM ((STEPSPERROTATION*MICROSTEPPING)/SCREWLEAD)
#define Y_STEPSPERMM ((STEPSPERROTATION*MICROSTEPPING)/SCREWLEAD)
#define Z_STEPSPERMM ((STEPSPERROTATION*MICROSTEPPING)/SCREWLEAD)
#define A_STEPSPERMM ((STEPSPERROTATION*MICROSTEPPING)/SCREWLEAD)
#define B_STEPSPERMM ((STEPSPERROTATION*MICROSTEPPING)/SCREWLEAD)
#define C_STEPSPERMM ((STEPSPERROTATION*MICROSTEPPING)/SCREWLEAD)

////////////////////////////////////////////////////////

#define CNC_MAXSPEED 20000         // steps/sec
#define CNC_ACC  500
#define CNC_DEC  550

////////////////////////////////////////////////////////
// NoReference, ReferenceToMin, ReferenceToMax

#define X_USEREFERENCE  EReverenceType::NoReference
#define Y_USEREFERENCE  EReverenceType::NoReference
#define Z_USEREFERENCE  EReverenceType::NoReference
#define A_USEREFERENCE  EReverenceType::NoReference
#define B_USEREFERENCE  EReverenceType::NoReference
#define C_USEREFERENCE  EReverenceType::NoReference

#define REFMOVE_1_AXIS  255
#define REFMOVE_2_AXIS  255
#define REFMOVE_3_AXIS  255
#define REFMOVE_4_AXIS  255
#define REFMOVE_5_AXIS  255
#define REFMOVE_6_AXIS  255

#define X_REFERENCEHITVALUE LOW
#define Y_REFERENCEHITVALUE LOW
#define Z_REFERENCEHITVALUE LOW
#define A_REFERENCEHITVALUE LOW
#define B_REFERENCEHITVALUE LOW
#define C_REFERENCEHITVALUE LOW

#define MOVEAWAYFROMREF_MM1000 250

#define SPINDLE_ANALOGSPEED
#define SPINDLE_ISLASER
#define SPINDLE_MAXSPEED	255			// analog 255

////////////////////////////////////////////////////////

#if STEPPERTYPE==4
#include "Configuration_CNCShield.h"
#endif

////////////////////////////////////////////////////////

// do not use probe
#undef PROBE_PIN

////////////////////////////////////////////////////////

#define GO_DEFAULT_STEPRATE		((steprate_t) CConfigEeprom::GetConfigU32(offsetof(CConfigEeprom::SCNCEeprom, maxsteprate)))	// steps/sec
#define G1_DEFAULT_MAXSTEPRATE	((steprate_t) CConfigEeprom::GetConfigU32(offsetof(CConfigEeprom::SCNCEeprom, maxsteprate)))	// steps/sec
#define G1_DEFAULT_FEEDPRATE	100000	// in mm1000 / min

#define STEPRATERATE_REFMOVE	(CNC_MAXSPEED/4)

////////////////////////////////////////////////////////

#define MESSAGE_MYCONTROL_Starting					F("MiniL:" __DATE__ )

