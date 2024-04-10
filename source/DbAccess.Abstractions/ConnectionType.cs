namespace DbAccess.Abstractions;

public abstract class ConnectionType;

public abstract class ReadOnly : ConnectionType;

public abstract class ReadWrite : ConnectionType;
