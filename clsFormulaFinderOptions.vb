Public Class clsFormulaFinderOptions

    Private mFindCharge As Boolean
    Private mLimitChargeRange As Boolean
    Private mComputeMZ As Boolean
    Private mFindTargetMZ As Boolean

    ''' <summary>
    ''' When true, compute the overall charge of each compound
    ''' </summary>
    ''' <remarks></remarks>
    Public Property FindCharge As Boolean
        Get
            Return mFindCharge
        End Get
        Set(value As Boolean)
            mFindCharge = value

            If mFindCharge = False Then
                ' Auto-disable a few options
                mLimitChargeRange = False
                mComputeMZ = False
                mFindTargetMZ = False
            End If

        End Set
    End Property

    ''' <summary>
    ''' When true, filter the results by ChargeMin and ChargeMax
    ''' </summary>
    ''' <remarks>
    ''' Setting this to True auto-sets FindCharge to true
    ''' Setting this to False auto-sets FindTargetMZ to false</remarks>
    Public Property LimitChargeRange As Boolean
        Get
            Return mLimitChargeRange
        End Get
        Set(value As Boolean)
            mLimitChargeRange = value
            If mLimitChargeRange Then
                FindCharge = True
            Else
                mFindTargetMZ = False
            End If
        End Set
    End Property

    ''' <summary>
    ''' When LimitChargeRange is true, results will be limited to the range ChargeMin to ChargeMax
    ''' </summary>
    ''' <remarks>Negative values are allowed</remarks>
    Public Property ChargeMin As Integer

    ''' <summary>
    ''' When LimitChargeRange is true, results will be limited to the range ChargeMin to ChargeMax
    ''' </summary>
    ''' <remarks>Negative values are allowed</remarks>
    Public Property ChargeMax As Integer

    ''' <summary>
    ''' Set to true to report the m/z value of identified compounds
    ''' </summary>
    ''' <remarks>Only valid if FindCharge is True.  Setting to false auto-sets FindTargetMZ to false</remarks>
    Public Property ComputeMZ As Boolean
        Get
            Return mComputeMZ
        End Get
        Set(value As Boolean)
            mComputeMZ = value
            If mComputeMZ = False Then
                mFindTargetMZ = False
            End If
        End Set
    End Property

    ''' <summary>
    ''' Set to true to search for a target m/z value instead of a target mass
    ''' </summary>
    ''' <remarks>Setting this to True auto-sets FindCharge and LimitChargeRange to True</remarks>
    Public Property FindTargetMZ As Boolean
        Get
            Return mFindTargetMZ
        End Get
        Set(value As Boolean)
            mFindTargetMZ = value
            If (mFindTargetMZ) Then
                FindCharge = True
                LimitChargeRange = True
            End If
        End Set
    End Property

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        FindCharge = False
        LimitChargeRange = False
        ChargeMin = -4
        ChargeMax = 4
        ComputeMZ = False
        FindTargetMZ = False
    End Sub

End Class
