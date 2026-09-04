global using Clinic_System.Data.Context;
global using Clinic_System.Infrastructure.Identity;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.IdentityModel.Tokens;
global using System.Text;
global using Clinic_System.Application;
global using Clinic_System.Data.Repository.UnitOfWork;
global using Clinic_System.Application.Common;
global using Clinic_System.Application.Features.Doctors.Queries.Models;
global using MediatR;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Clinic_System.Application.Service.Implemention;
global using Clinic_System.Application.Service.Interface;
global using Clinic_System.Infrastructure.Services;
global using Clinic_System.Application.Features.Doctors.Commands.Models;
global using Clinic_System.API.Bases;
global using Clinic_System.Application.Common.Bases;
global using System.Net;
global using FluentValidation;
global using System.Text.Json;
global using Clinic_System.API.Middlewares;
global using Clinic_System.Application.Common.Behaviors;
global using Clinic_System.Core.Exceptions;
global using Clinic_System.Application.Features.Appointments.Queries.Models;
global using Clinic_System.Application.Features.Appointments.Commands.Models;
global using Serilog;
global using Clinic_System.Application.Features.Patients.Queries.Models;
global using Clinic_System.Application.Features.Patients.Commands.Models;
global using Clinic_System.Core.Enums;
global using Clinic_System.Infrastructure.Helpers;
global using Hangfire;
global using Clinic_System.Infrastructure.Services.Email;
global using Clinic_System.Data;
global using Clinic_System.Data.Seed;
global using Clinic_System.Infrastructure;
global using Clinic_System.Application.Features.MedicalRecords.Commands.Models;
global using Clinic_System.Application.Features.MedicalRecords.Queries.Models;
global using Clinic_System.Application.Features.Prescriptions.Commands.Models;
global using Clinic_System.Application.Features.Payment.Queries.Models;
global using Clinic_System.Application.Features.Payment.Commands.Models;
global using Clinic_System.Application.Features.Authentication.Commands.Models;
global using Clinic_System.Application.Features.Authorization.Commands.Models;
global using Microsoft.OpenApi.Models;
global using Microsoft.AspNetCore.Authorization;
global using Clinic_System.Application.Features.Authentication.Queries.Models;
global using Clinic_System.Application.Features.DentalHistory.Commands.Models;
global using Clinic_System.Application.Features.DentalHistory.Queries.Models;
global using Clinic_System.Application.Features.ToothRecords.Commands.Models;
global using Clinic_System.Application.Features.ToothRecords.Queries.Models;
global using Clinic_System.Application.Features.ToothChart.Models;
global using Clinic_System.Application.Features.Periodontogram.Models;
global using Clinic_System.Application.Features.PatientPrescriptions.Models;
global using Clinic_System.Application.Features.DentalTreatments.Commands.Models;
global using Clinic_System.Application.Features.DentalTreatments.Queries.Models;
global using Clinic_System.Application.Features.TreatmentPlans.Commands.Models;
global using Clinic_System.Application.Features.TreatmentPlans.Queries.Models;
global using Clinic_System.Application.Features.InvoiceLines.Commands.Models;
global using Clinic_System.Application.Features.InvoiceLines.Queries.Models;
global using Clinic_System.Application.Features.TreatmentProcedures.Commands.Models;
global using Clinic_System.Application.Features.TreatmentProcedures.Queries.Models;
global using Clinic_System.Application.Features.MedicalConditions.Commands.Models;
global using Clinic_System.Application.Features.MedicalConditions.Queries.Models;
global using Clinic_System.Infrastructure.Authentication.Models;
global using System.Threading.RateLimiting;
global using Microsoft.AspNetCore.RateLimiting;
global using System.Security.Claims;
global using Clinic_System.API.Extensions;
global using Microsoft.AspNetCore.SignalR;
global using Clinic_System.API.Hubs;
global using Clinic_System.Application.DTOs;
global using Clinic_System.Infrastructure.MessageBroker.Consumers;
global using Clinic_System.Infrastructure.MessageBroker;
global using MassTransit;
global using Clinic_System.Application.Features.AccessControl.Queries.Models;
global using Clinic_System.Application.Features.AccessControl.Commands.Models;
global using Clinic_System.Application.Features.Dashboard.Models;
global using Clinic_System.API.Services;











