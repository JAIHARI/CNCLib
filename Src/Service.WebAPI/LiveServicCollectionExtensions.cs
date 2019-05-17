﻿/*
  This file is part of CNCLib - A library for stepper motors.

  Copyright (c) Herbert Aitenbichler

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/

using System;
using System.Net.Http;

using CNCLib.Service.Abstraction;

using Framework.Dependency;
using Framework.Service.WebAPI;

using Microsoft.Extensions.DependencyInjection;

namespace CNCLib.Service.WebAPI
{
    public static class LiveServicCollectionExtensions
    {
        public static IServiceCollection AddServiceAsWebAPI(this IServiceCollection services, Action<HttpClient> configureHttpClient)
        {
//            services.AddAssembylIncludingInternals(ServiceLifetime.Transient, typeof(MachineService).Assembly);

            services.AddHttpClient<IMachineService, MachineService>(configureHttpClient).AddConfigureClientBuilder();
            services.AddHttpClient<IEepromConfigurationService, EepromConfigurationService>(configureHttpClient).AddConfigureClientBuilder();
            services.AddHttpClient<IItemService, ItemService>(configureHttpClient).AddConfigureClientBuilder();
            services.AddHttpClient<ILoadOptionsService, LoadOptionsService>(configureHttpClient).AddConfigureClientBuilder();
            services.AddHttpClient<IUserService, UserService>(configureHttpClient).AddConfigureClientBuilder();

            return services;
        }

        private static void ConfigureClient(HttpClient httpClient)
        {
            HttpClientHelper.PrepareHttpClient(httpClient, @"http://cnclibwebapi.azurewebsites.net");
        }

        private static IHttpClientBuilder AddConfigureClientBuilder(this IHttpClientBuilder builder)
        {
            return builder.ConfigurePrimaryHttpMessageHandler(HttpClientHelper.CreateHttpClientHandlerIgnoreSSLCertificatesError);
        }
    }
}