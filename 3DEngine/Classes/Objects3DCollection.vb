Imports N3DEngine

Public Class Objects3DCollection
    Implements IDictionary(Of String, Object3D)

    Public Class ItemChangedEventArgs
        Inherits EventArgs

        Public Enum Actions
            ItemAdded
            ItemRemoved
            CollectionCleared
        End Enum

        Public ReadOnly Property Item As KeyValuePair(Of String, Object3D)
        Public ReadOnly Property Action As Actions

        Public Sub New(item As KeyValuePair(Of String, Object3D), action As Actions)
            Me.Item = item
            Me.Action = action
        End Sub
    End Class

    Private mCol As New Dictionary(Of String, Object3D)

    Public Event ItemChanged(sender As Object, e As ItemChangedEventArgs)

    Public ReadOnly Property Count As Integer Implements ICollection(Of KeyValuePair(Of String, Object3D)).Count
        Get
            Return mCol.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of KeyValuePair(Of String, Object3D)).IsReadOnly
        Get
            Return False
        End Get
    End Property

    Default Public Property Item(key As String) As Object3D Implements IDictionary(Of String, Object3D).Item
        Get
            Return mCol(key)
        End Get
        Set(value As Object3D)
            mCol(key) = value
        End Set
    End Property

    Default Public Property Item(index As Integer) As Object3D
        Get
            Dim i As Integer = 0
            For Each itm In mCol
                If i = index Then Return itm.Value
                i += 1
            Next
            Throw New ArgumentOutOfRangeException()
        End Get
        Set(value As Object3D)
            Dim i As Integer = 0
            For Each itm In mCol
                If i = index Then
                    mCol(itm.Key) = value
                    Exit Property
                End If
                i += 1
            Next
            Throw New ArgumentOutOfRangeException()
        End Set
    End Property

    Public ReadOnly Property Keys As ICollection(Of String) Implements IDictionary(Of String, Object3D).Keys
        Get
            Return mCol.Keys
        End Get
    End Property

    Public ReadOnly Property Values As ICollection(Of Object3D) Implements IDictionary(Of String, Object3D).Values
        Get
            Return mCol.Values
        End Get
    End Property

    Public Sub Add(item As KeyValuePair(Of String, Object3D)) Implements ICollection(Of KeyValuePair(Of String, Object3D)).Add
        mCol.Add(item.Key, item.Value)
        RaiseEvent ItemChanged(Me, New ItemChangedEventArgs(mCol.Last(), ItemChangedEventArgs.Actions.ItemAdded))
    End Sub

    Public Sub Add(key As String, value As Object3D) Implements IDictionary(Of String, Object3D).Add
        mCol.Add(key, value)
        RaiseEvent ItemChanged(Me, New ItemChangedEventArgs(mCol.Last(), ItemChangedEventArgs.Actions.ItemAdded))
    End Sub

    Public Sub Clear() Implements ICollection(Of KeyValuePair(Of String, Object3D)).Clear
        mCol.Clear()
        RaiseEvent ItemChanged(Me, New ItemChangedEventArgs(Nothing, ItemChangedEventArgs.Actions.CollectionCleared))
    End Sub

    Public Sub CopyTo(array() As KeyValuePair(Of String, Object3D), arrayIndex As Integer) Implements ICollection(Of KeyValuePair(Of String, Object3D)).CopyTo
        mCol.ToArray().CopyTo(array, arrayIndex)
    End Sub

    Public Function Contains(item As KeyValuePair(Of String, Object3D)) As Boolean Implements ICollection(Of KeyValuePair(Of String, Object3D)).Contains
        Return mCol.Contains(item)
    End Function

    Public Function ContainsKey(key As String) As Boolean Implements IDictionary(Of String, Object3D).ContainsKey
        Return mCol.ContainsKey(key)
    End Function

    Public Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, Object3D)) Implements IEnumerable(Of KeyValuePair(Of String, Object3D)).GetEnumerator
        Return mCol.GetEnumerator()
    End Function

    Public Function Remove(item As KeyValuePair(Of String, Object3D)) As Boolean Implements ICollection(Of KeyValuePair(Of String, Object3D)).Remove
        If mCol.Remove(item.Key) Then
            RaiseEvent ItemChanged(Me, New ItemChangedEventArgs(item, ItemChangedEventArgs.Actions.ItemRemoved))
            Return True
        End If

        Return False
    End Function

    Public Function Remove(key As String) As Boolean Implements IDictionary(Of String, Object3D).Remove
        If mCol.ContainsKey(key) Then
            Dim kvpItem = mCol.Where(Function(kvp) kvp.Key = key).Single()
            If mCol.Remove(key) Then
                RaiseEvent ItemChanged(Me, New ItemChangedEventArgs(kvpItem, ItemChangedEventArgs.Actions.ItemRemoved))
                Return True
            End If
        End If

        Return False
    End Function

    Public Function TryGetValue(key As String, ByRef value As Object3D) As Boolean Implements IDictionary(Of String, Object3D).TryGetValue
        Return mCol.TryGetValue(key, value)
    End Function

    Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return mCol.GetEnumerator()
    End Function

    Public Sub ForEach(action As Action(Of KeyValuePair(Of String, Object3D)))
        For Each kvp In mCol
            action(kvp)
        Next
    End Sub
End Class
