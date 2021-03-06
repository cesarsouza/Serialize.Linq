﻿#region Copyright
//  Copyright, Sascha Kiefer (esskar)
//  Released under LGPL License.
//  
//  License: https://raw.github.com/esskar/Serialize.Linq/master/LICENSE
//  Contributing: https://github.com/esskar/Serialize.Linq
#endregion

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Serialize.Linq.Exceptions;
using Serialize.Linq.Interfaces;
using Serialize.Linq.Internals;

namespace Serialize.Linq.Nodes
{
    #region DataContract
#if !SERIALIZE_LINQ_OPTIMIZE_SIZE
    [DataContract]
#else
    [DataContract(Name = "C")]   
#endif
#if !SILVERLIGHT
    [Serializable]
#endif
    #endregion
    public class ConstantExpressionNode : ExpressionNode<ConstantExpression>
    {
        private object _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpressionNode"/> class.
        /// </summary>
        public ConstantExpressionNode() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpressionNode"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="value">The value.</param>
        public ConstantExpressionNode(INodeFactory factory, object value)
            : base(factory, ExpressionType.Constant)
        {
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpressionNode"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="expression">The expression.</param>
        public ConstantExpressionNode(INodeFactory factory, ConstantExpression expression)
            : base(factory, expression) { }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        /// <exception cref="InvalidTypeException"></exception>
        public override TypeNode Type
        {
            get { return base.Type; }
            set
            {
                if (this.Value != null)
                {
                    if (value == null)
                        value = this.Factory.Create(this.Value.GetType());
                    else
                    {
                        var context = new ExpressionContext();
                        if (!value.ToType(context).IsInstanceOfType(this.Value))
                            throw new InvalidTypeException(string.Format("Type '{0}' is not an instance of the current value type '{1}'.", value.ToType(context), this.Value.GetType()));
                    }
                }
                base.Type = value;
            }
        }

        #region DataMember
        #if !SERIALIZE_LINQ_OPTIMIZE_SIZE
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <exception cref="System.ArgumentException">Expression not allowed.;value</exception>
        [DataMember(EmitDefaultValue = false)]
#else
        [DataMember(EmitDefaultValue = false, Name = "V")]
#endif
        #endregion
        public object Value
        {
            get { return _value; }
            set
            {
                if (value is Expression)
                    throw new ArgumentException("Expression not allowed.", "value");
                _value = value;
                if (_value != null)
                {
                    var type = base.Type != null ? base.Type.ToType(new ExpressionContext()) : null;
                    if (type == null)
                    {
                        if(this.Factory != null)
                            base.Type = this.Factory.Create(_value.GetType());
                        return;
                    }
                    _value = ValueConverter.Convert(_value, type);
                }
            }
        }

        /// <summary>
        /// Initializes the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected override void Initialize(ConstantExpression expression)
        {
            this.Value = expression.Value;
        }

        /// <summary>
        /// Converts this instance to an expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override Expression ToExpression(ExpressionContext context)
        {
            return this.Type != null ? Expression.Constant(this.Value, this.Type.ToType(context)) : Expression.Constant(this.Value);
        }
    }
}
