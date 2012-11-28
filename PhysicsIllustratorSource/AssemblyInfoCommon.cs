//--------------------------------------------------------------------------
// 
//  Copyright (C) 2004 Microsoft Corporation
//  All rights reserved.
// 
//  File: AssemblyInfoCommon.cs
//  
//  Description: Assembly-level attributes common to all projects in the solution.  
//  (See also AssemblyInfoSpecific.cs in each project subfolder, for -Title, -Description, 
//  and security related assembly attributes.)
//
//--------------------------------------------------------------------------
using System;
using System.Reflection;

// Define attributes common to all assemblies in this product.
[assembly: AssemblyProduct("CoPhysics Illustrator for Tablet PC")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyCopyright("Copyright � 2004 Microsoft Corporation")]
[assembly: AssemblyTrademark("Microsoft � is a registered trademark of Microsoft Corporation. Windows� is a trademark of Microsoft Corporation.")]

// Set major/minor version, autogenerate build/revision fields.
[assembly: AssemblyVersion("3.0.*")]

//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified, the assembly is not signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. KeyFile refers to a file which contains
//       a key.
//   (*) If the KeyFile and the KeyName values are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP, that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the KeyFile is installed into the CSP and used.
//   (*) In order to create a KeyFile, you can use the sn.exe (Strong Name) utility.
//       When specifying the KeyFile, the location of the KeyFile should be
//       relative to the project output directory which is
//       %Project Directory%\obj\<configuration>. For example, if your KeyFile is
//       located in the project directory, you would specify the AssemblyKeyFile 
//       attribute as [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("")]
[assembly: AssemblyKeyName("")]