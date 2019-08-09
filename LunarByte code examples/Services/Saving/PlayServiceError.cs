using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum PlayServiceError : byte
{
	None = 0,
	Timeout = 1,
	NotAuthenticated = 2,
	SaveGameNotEnabled = 4,
	CloudSaveNameNotSet = 8,
}