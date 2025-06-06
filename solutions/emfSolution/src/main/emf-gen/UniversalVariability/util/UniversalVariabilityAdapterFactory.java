/**
 */
package UniversalVariability.util;

import UniversalVariability.*;

import org.eclipse.emf.common.notify.Adapter;
import org.eclipse.emf.common.notify.Notifier;

import org.eclipse.emf.common.notify.impl.AdapterFactoryImpl;

import org.eclipse.emf.ecore.EObject;

/**
 * <!-- begin-user-doc -->
 * The <b>Adapter Factory</b> for the model.
 * It provides an adapter <code>createXXX</code> method for each class of the model.
 * <!-- end-user-doc -->
 * @see UniversalVariability.UniversalVariabilityPackage
 * @generated
 */
public class UniversalVariabilityAdapterFactory extends AdapterFactoryImpl {
	/**
	 * The cached model package.
	 * <!-- begin-user-doc -->
	 * <!-- end-user-doc -->
	 * @generated
	 */
	protected static UniversalVariabilityPackage modelPackage;

	/**
	 * Creates an instance of the adapter factory.
	 * <!-- begin-user-doc -->
	 * <!-- end-user-doc -->
	 * @generated
	 */
	public UniversalVariabilityAdapterFactory() {
		if (modelPackage == null) {
			modelPackage = UniversalVariabilityPackage.eINSTANCE;
		}
	}

	/**
	 * Returns whether this factory is applicable for the type of the object.
	 * <!-- begin-user-doc -->
	 * This implementation returns <code>true</code> if the object is either the model's package or is an instance object of the model.
	 * <!-- end-user-doc -->
	 * @return whether this factory is applicable for the type of the object.
	 * @generated
	 */
	@Override
	public boolean isFactoryForType(Object object) {
		if (object == modelPackage) {
			return true;
		}
		if (object instanceof EObject) {
			return ((EObject)object).eClass().getEPackage() == modelPackage;
		}
		return false;
	}

	/**
	 * The switch that delegates to the <code>createXXX</code> methods.
	 * <!-- begin-user-doc -->
	 * <!-- end-user-doc -->
	 * @generated
	 */
	protected UniversalVariabilitySwitch<Adapter> modelSwitch =
		new UniversalVariabilitySwitch<Adapter>() {
			@Override
			public Adapter caseFeatureModel(FeatureModel object) {
				return createFeatureModelAdapter();
			}
			@Override
			public Adapter caseFeature(Feature object) {
				return createFeatureAdapter();
			}
			@Override
			public Adapter caseConstraint(Constraint object) {
				return createConstraintAdapter();
			}
			@Override
			public Adapter caseFeatureGroup(FeatureGroup object) {
				return createFeatureGroupAdapter();
			}
			@Override
			public Adapter caseOrFeatureGroup(OrFeatureGroup object) {
				return createOrFeatureGroupAdapter();
			}
			@Override
			public Adapter caseMandatoryFeatureGroup(MandatoryFeatureGroup object) {
				return createMandatoryFeatureGroupAdapter();
			}
			@Override
			public Adapter caseOptionalFeatureGroup(OptionalFeatureGroup object) {
				return createOptionalFeatureGroupAdapter();
			}
			@Override
			public Adapter caseAlternativeFeatureGroup(AlternativeFeatureGroup object) {
				return createAlternativeFeatureGroupAdapter();
			}
			@Override
			public Adapter caseImpliesConstraint(ImpliesConstraint object) {
				return createImpliesConstraintAdapter();
			}
			@Override
			public Adapter caseEquivalenceConstraint(EquivalenceConstraint object) {
				return createEquivalenceConstraintAdapter();
			}
			@Override
			public Adapter caseBinaryConstraint(BinaryConstraint object) {
				return createBinaryConstraintAdapter();
			}
			@Override
			public Adapter caseAndConstraint(AndConstraint object) {
				return createAndConstraintAdapter();
			}
			@Override
			public Adapter caseOrConstraint(OrConstraint object) {
				return createOrConstraintAdapter();
			}
			@Override
			public Adapter caseFeatureConstraint(FeatureConstraint object) {
				return createFeatureConstraintAdapter();
			}
			@Override
			public Adapter caseNotConstraint(NotConstraint object) {
				return createNotConstraintAdapter();
			}
			@Override
			public Adapter defaultCase(EObject object) {
				return createEObjectAdapter();
			}
		};

	/**
	 * Creates an adapter for the <code>target</code>.
	 * <!-- begin-user-doc -->
	 * <!-- end-user-doc -->
	 * @param target the object to adapt.
	 * @return the adapter for the <code>target</code>.
	 * @generated
	 */
	@Override
	public Adapter createAdapter(Notifier target) {
		return modelSwitch.doSwitch((EObject)target);
	}


	/**
	 * Creates a new adapter for an object of class '{@link UniversalVariability.FeatureModel <em>Feature Model</em>}'.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null so that we can easily ignore cases;
	 * it's useful to ignore a case when inheritance will catch all the cases anyway.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @see UniversalVariability.FeatureModel
	 * @generated
	 */
	public Adapter createFeatureModelAdapter() {
		return null;
	}

	/**
	 * Creates a new adapter for an object of class '{@link UniversalVariability.Feature <em>Feature</em>}'.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null so that we can easily ignore cases;
	 * it's useful to ignore a case when inheritance will catch all the cases anyway.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @see UniversalVariability.Feature
	 * @generated
	 */
	public Adapter createFeatureAdapter() {
		return null;
	}

	/**
	 * Creates a new adapter for an object of class '{@link UniversalVariability.Constraint <em>Constraint</em>}'.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null so that we can easily ignore cases;
	 * it's useful to ignore a case when inheritance will catch all the cases anyway.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @see UniversalVariability.Constraint
	 * @generated
	 */
	public Adapter createConstraintAdapter() {
		return null;
	}

	/**
	 * Creates a new adapter for an object of class '{@link UniversalVariability.FeatureGroup <em>Feature Group</em>}'.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null so that we can easily ignore cases;
	 * it's useful to ignore a case when inheritance will catch all the cases anyway.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @see UniversalVariability.FeatureGroup
	 * @generated
	 */
	public Adapter createFeatureGroupAdapter() {
		return null;
	}

	/**
	 * Creates a new adapter for an object of class '{@link UniversalVariability.OrFeatureGroup <em>Or Feature Group</em>}'.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null so that we can easily ignore cases;
	 * it's useful to ignore a case when inheritance will catch all the cases anyway.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @see UniversalVariability.OrFeatureGroup
	 * @generated
	 */
	public Adapter createOrFeatureGroupAdapter() {
		return null;
	}

	/**
	 * Creates a new adapter for an object of class '{@link UniversalVariability.MandatoryFeatureGroup <em>Mandatory Feature Group</em>}'.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null so that we can easily ignore cases;
	 * it's useful to ignore a case when inheritance will catch all the cases anyway.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @see UniversalVariability.MandatoryFeatureGroup
	 * @generated
	 */
	public Adapter createMandatoryFeatureGroupAdapter() {
		return null;
	}

	/**
	 * Creates a new adapter for an object of class '{@link UniversalVariability.OptionalFeatureGroup <em>Optional Feature Group</em>}'.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null so that we can easily ignore cases;
	 * it's useful to ignore a case when inheritance will catch all the cases anyway.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @see UniversalVariability.OptionalFeatureGroup
	 * @generated
	 */
	public Adapter createOptionalFeatureGroupAdapter() {
		return null;
	}

	/**
	 * Creates a new adapter for an object of class '{@link UniversalVariability.AlternativeFeatureGroup <em>Alternative Feature Group</em>}'.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null so that we can easily ignore cases;
	 * it's useful to ignore a case when inheritance will catch all the cases anyway.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @see UniversalVariability.AlternativeFeatureGroup
	 * @generated
	 */
	public Adapter createAlternativeFeatureGroupAdapter() {
		return null;
	}

	/**
	 * Creates a new adapter for an object of class '{@link UniversalVariability.ImpliesConstraint <em>Implies Constraint</em>}'.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null so that we can easily ignore cases;
	 * it's useful to ignore a case when inheritance will catch all the cases anyway.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @see UniversalVariability.ImpliesConstraint
	 * @generated
	 */
	public Adapter createImpliesConstraintAdapter() {
		return null;
	}

	/**
	 * Creates a new adapter for an object of class '{@link UniversalVariability.EquivalenceConstraint <em>Equivalence Constraint</em>}'.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null so that we can easily ignore cases;
	 * it's useful to ignore a case when inheritance will catch all the cases anyway.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @see UniversalVariability.EquivalenceConstraint
	 * @generated
	 */
	public Adapter createEquivalenceConstraintAdapter() {
		return null;
	}

	/**
	 * Creates a new adapter for an object of class '{@link UniversalVariability.BinaryConstraint <em>Binary Constraint</em>}'.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null so that we can easily ignore cases;
	 * it's useful to ignore a case when inheritance will catch all the cases anyway.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @see UniversalVariability.BinaryConstraint
	 * @generated
	 */
	public Adapter createBinaryConstraintAdapter() {
		return null;
	}

	/**
	 * Creates a new adapter for an object of class '{@link UniversalVariability.AndConstraint <em>And Constraint</em>}'.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null so that we can easily ignore cases;
	 * it's useful to ignore a case when inheritance will catch all the cases anyway.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @see UniversalVariability.AndConstraint
	 * @generated
	 */
	public Adapter createAndConstraintAdapter() {
		return null;
	}

	/**
	 * Creates a new adapter for an object of class '{@link UniversalVariability.OrConstraint <em>Or Constraint</em>}'.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null so that we can easily ignore cases;
	 * it's useful to ignore a case when inheritance will catch all the cases anyway.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @see UniversalVariability.OrConstraint
	 * @generated
	 */
	public Adapter createOrConstraintAdapter() {
		return null;
	}

	/**
	 * Creates a new adapter for an object of class '{@link UniversalVariability.FeatureConstraint <em>Feature Constraint</em>}'.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null so that we can easily ignore cases;
	 * it's useful to ignore a case when inheritance will catch all the cases anyway.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @see UniversalVariability.FeatureConstraint
	 * @generated
	 */
	public Adapter createFeatureConstraintAdapter() {
		return null;
	}

	/**
	 * Creates a new adapter for an object of class '{@link UniversalVariability.NotConstraint <em>Not Constraint</em>}'.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null so that we can easily ignore cases;
	 * it's useful to ignore a case when inheritance will catch all the cases anyway.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @see UniversalVariability.NotConstraint
	 * @generated
	 */
	public Adapter createNotConstraintAdapter() {
		return null;
	}

	/**
	 * Creates a new adapter for the default case.
	 * <!-- begin-user-doc -->
	 * This default implementation returns null.
	 * <!-- end-user-doc -->
	 * @return the new adapter.
	 * @generated
	 */
	public Adapter createEObjectAdapter() {
		return null;
	}

} //UniversalVariabilityAdapterFactory
