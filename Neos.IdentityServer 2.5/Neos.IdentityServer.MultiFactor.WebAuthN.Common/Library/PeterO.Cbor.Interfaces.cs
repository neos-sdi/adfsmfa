using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor
{
    #region ICBORConverter
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:ICBORConverter`1"]/*'/>
    public interface ICBORConverter<T>
    {
        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:ICBORConverter`1.ToCBORObject(`0)"]/*'/>
        CBORObject ToCBORObject(T obj);
    }
    #endregion

    #region ICBORNumber
    internal interface ICBORNumber
    {
        bool IsPositiveInfinity(Object obj);

        bool IsInfinity(Object obj);

        bool IsNegativeInfinity(Object obj);

        bool IsNaN(Object obj);

        bool IsNegative(Object obj);

        double AsDouble(Object obj);

        object Negate(Object obj);

        object Abs(Object obj);

        EDecimal AsExtendedDecimal(Object obj);

        EFloat AsExtendedFloat(Object obj);

        ERational AsExtendedRational(Object obj);

        float AsSingle(Object obj);

        EInteger AsEInteger(Object obj);

        long AsInt64(Object obj);

        bool CanFitInSingle(Object obj);

        bool CanFitInDouble(Object obj);

        bool CanFitInInt32(Object obj);

        bool CanFitInInt64(Object obj);

        bool CanTruncatedIntFitInInt64(Object obj);

        bool CanTruncatedIntFitInInt32(Object obj);

        int AsInt32(Object obj, int minValue, int maxValue);

        bool IsZero(Object obj);

        int Sign(Object obj);

        bool IsIntegral(Object obj);
    }
    #endregion

    #region ICBORTag
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:ICBORTag"]/*'/>
    public interface ICBORTag
    {
        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:ICBORTag.GetTypeFilter"]/*'/>
        CBORTypeFilter GetTypeFilter();

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:ICBORTag.ValidateObject(CBORObject)"]/*'/>
        CBORObject ValidateObject(CBORObject obj);
    }
    #endregion

    #region ICBORToFromConverter
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:ICBORObjectConverter`1"]/*'/>
    public interface ICBORToFromConverter<T> : ICBORConverter<T>
    {
        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:ICBORObjectConverter`1.FromCBORObject(CBORObject)"]/*'/>
        T FromCBORObject(CBORObject cbor);
    }
    #endregion

    
    #region ICharacterInput
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:ICharacterInput"]/*'/>
    internal interface ICharacterInput
    {
        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:ICharacterInput.ReadChar"]/*'/>
        int ReadChar();

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:ICharacterInput.Read(System.Int32[],System.Int32,System.Int32)"]/*'/>
        int Read(int[] chars, int index, int length);
    }
    #endregion
}
