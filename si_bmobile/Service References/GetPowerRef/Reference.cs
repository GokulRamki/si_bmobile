﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace si_bmobile.GetPowerRef {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="GetPowerRef.IGetPower")]
    public interface IGetPower {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IGetPower/lookup", ReplyAction="http://tempuri.org/IGetPower/lookupResponse")]
        string lookup(string serUserName, string serPassword, string Msisdn, string data, int iphone);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IGetPower/GetToken", ReplyAction="http://tempuri.org/IGetPower/GetTokenResponse")]
        string GetToken(string serUserName, string serPassword, string Msisdn, string data, string amount, int iphone);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IGetPowerChannel : si_bmobile.GetPowerRef.IGetPower, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetPowerClient : System.ServiceModel.ClientBase<si_bmobile.GetPowerRef.IGetPower>, si_bmobile.GetPowerRef.IGetPower {
        
        public GetPowerClient() {
        }
        
        public GetPowerClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public GetPowerClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public GetPowerClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public GetPowerClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string lookup(string serUserName, string serPassword, string Msisdn, string data, int iphone) {
            return base.Channel.lookup(serUserName, serPassword, Msisdn, data, iphone);
        }
        
        public string GetToken(string serUserName, string serPassword, string Msisdn, string data, string amount, int iphone) {
            return base.Channel.GetToken(serUserName, serPassword, Msisdn, data, amount, iphone);
        }
    }
}