 using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartWarehouse.Core;

namespace SmartWarehouse.Tests;

[TestClass]
public class WarehouseSensorTests
{
    [TestMethod]
    public void Constructor_ShouldSetInitialState()
    {
        // Arrange
        string sensorId = "SENSOR-001";
        string locationTag = "Cold-Vault-01";

        // Act
        var sensor = new WarehouseSensor(sensorId, locationTag);

        // Assert
        Assert.AreEqual(sensorId, sensor.SensorId);
        Assert.AreEqual(locationTag, sensor.LocationTag);
        Assert.AreEqual(0.0, sensor.CurrentTemperature);
        Assert.IsFalse(sensor.IsActive);
        Assert.IsFalse(sensor.IsAlertTriggered);
        Assert.AreEqual(4.0, sensor.CriticalThresholdCelsius);
    }

    [TestMethod]
    public void Constructor_ShouldAcceptCustomThreshold()
    {
        // Arrange
        double threshold = 10.0;

        // Act
        var sensor = new WarehouseSensor(
            "SENSOR-002",
            "Storage-A",
            threshold);

        // Assert
        Assert.AreEqual(threshold, sensor.CriticalThresholdCelsius);
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_ShouldThrowArgumentExceptionForInvalidSensorId(
        string? sensorId)
    {
        // Arrange
        string locationTag = "Cold-Vault-01";

        // Act
        Action act = () => new WarehouseSensor(
            sensorId!,
            locationTag);

        // Assert
        Assert.ThrowsExactly<ArgumentException>(act);
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_ShouldThrowArgumentExceptionForInvalidLocationTag(
        string? locationTag)
    {
        // Arrange
        string sensorId = "SENSOR-001";

        // Act
        Action act = () => new WarehouseSensor(
            sensorId,
            locationTag!);

        // Assert
        Assert.ThrowsExactly<ArgumentException>(act);
    }

    [TestMethod]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var sensor = new WarehouseSensor(
            "SENSOR-001",
            "Cold-Vault-01");

        // Act
        sensor.Activate();

        // Assert
        Assert.IsTrue(sensor.IsActive);
    }

    [TestMethod]
    public void Activate_ShouldBeIdempotent()
    {
        // Arrange
        var sensor = new WarehouseSensor(
            "SENSOR-001",
            "Cold-Vault-01");

        // Act
        sensor.Activate();
        sensor.Activate();

        // Assert
        Assert.IsTrue(sensor.IsActive);
    }

    [TestMethod]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var sensor = new WarehouseSensor(
            "SENSOR-001",
            "Cold-Vault-01");

        sensor.Activate();

        // Act
        sensor.Deactivate();

        // Assert
        Assert.IsFalse(sensor.IsActive);
    }

    [TestMethod]
    public void Deactivate_ShouldResetAlertState()
    {
        // Arrange
        var sensor = new WarehouseSensor(
            "SENSOR-001",
            "Cold-Vault-01");

        sensor.Activate();
        sensor.RecordReading(10.0);

        // Act
        sensor.Deactivate();

        // Assert
        Assert.IsFalse(sensor.IsActive);
        Assert.IsFalse(sensor.IsAlertTriggered);
    }

    [TestMethod]
    public void RecordReading_WhenSensorIsInactive_ShouldThrowException()
    {
        // Arrange
        var sensor = new WarehouseSensor(
            "SENSOR-001",
            "Cold-Vault-01");

        // Act
        Action act = () => sensor.RecordReading(5.0);

        // Assert
        Assert.ThrowsExactly<InvalidOperationException>(act);
    }

    [DataTestMethod]
    [DataRow(3.9, false)]
    [DataRow(4.0, true)]
    [DataRow(4.1, true)]
    public void RecordReading_ShouldSetAlertCorrectlyAroundThreshold(
        double temperature,
        bool expectedAlert)
    {
        // Arrange
        var sensor = new WarehouseSensor(
            "SENSOR-001",
            "Cold-Vault-01");

        sensor.Activate();

        // Act
        sensor.RecordReading(temperature);

        // Assert
        Assert.AreEqual(temperature, sensor.CurrentTemperature);
        Assert.AreEqual(expectedAlert, sensor.IsAlertTriggered);
    }

    [DataTestMethod]
    [DataRow(-50.0)]
    [DataRow(80.0)]
    public void RecordReading_ShouldAcceptBoundaryTemperatures(
        double temperature)
    {
        // Arrange
        var sensor = new WarehouseSensor(
            "SENSOR-001",
            "Cold-Vault-01");

        sensor.Activate();

        // Act
        sensor.RecordReading(temperature);

        // Assert
        Assert.AreEqual(temperature, sensor.CurrentTemperature);
    }

    [DataTestMethod]
    [DataRow(-50.1)]
    [DataRow(80.1)]
    public void RecordReading_ShouldRejectOutOfRangeTemperatures(
        double temperature)
    {
        // Arrange
        var sensor = new WarehouseSensor(
            "SENSOR-001",
            "Cold-Vault-01");

        sensor.Activate();

        // Act
        Action act = () => sensor.RecordReading(temperature);

        // Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(act);
    }

    [TestMethod]
    public void RecordReading_ShouldUpdateCurrentTemperature()
    {
        // Arrange
        var sensor = new WarehouseSensor(
            "SENSOR-001",
            "Cold-Vault-01");

        sensor.Activate();

        // Act
        sensor.RecordReading(2.5);

        // Assert
        Assert.AreEqual(2.5, sensor.CurrentTemperature);
    }

    [TestMethod]
    public void UpdateThreshold_ShouldUpdateCriticalThreshold()
    {
        // Arrange
        var sensor = new WarehouseSensor(
            "SENSOR-001",
            "Cold-Vault-01");

        // Act
        sensor.UpdateThreshold(10.0);

        // Assert
        Assert.AreEqual(10.0, sensor.CriticalThresholdCelsius);
    }

    [DataTestMethod]
    [DataRow(-30.0)]
    [DataRow(50.0)]
    public void UpdateThreshold_ShouldAcceptBoundaryValues(
        double threshold)
    {
        // Arrange
        var sensor = new WarehouseSensor(
            "SENSOR-001",
            "Cold-Vault-01");

        // Act
        sensor.UpdateThreshold(threshold);

        // Assert
        Assert.AreEqual(threshold, sensor.CriticalThresholdCelsius);
    }

    [DataTestMethod]
    [DataRow(-30.1)]
    [DataRow(50.1)]
    public void UpdateThreshold_ShouldRejectOutOfRangeValues(
        double threshold)
    {
        // Arrange
        var sensor = new WarehouseSensor(
            "SENSOR-001",
            "Cold-Vault-01");

        // Act
        Action act = () => sensor.UpdateThreshold(threshold);

        // Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(act);
    }

    [TestMethod]
    public void UpdateThreshold_ShouldClearAlertWhenNewThresholdIsAboveTemperature()
    {
        // Arrange
        var sensor = new WarehouseSensor(
            "SENSOR-001",
            "Cold-Vault-01");

        sensor.Activate();
        sensor.RecordReading(6.0);

        // Act
        sensor.UpdateThreshold(8.0);

        // Assert
        Assert.IsFalse(sensor.IsAlertTriggered);
    }

    [TestMethod]
    public void UpdateThreshold_ShouldTriggerAlertWhenNewThresholdIsBelowTemperature()
    {
        // Arrange
        var sensor = new WarehouseSensor(
            "SENSOR-001",
            "Cold-Vault-01");

        sensor.Activate();
        sensor.RecordReading(6.0);

        // Act
        sensor.UpdateThreshold(5.0);

        // Assert
        Assert.IsTrue(sensor.IsAlertTriggered);
    }

    [TestMethod]
    public void UpdateThreshold_ShouldTriggerAlertWhenTemperatureEqualsNewThreshold()
    {
        // Arrange
        var sensor = new WarehouseSensor(
            "SENSOR-001",
            "Cold-Vault-01");

        sensor.Activate();
        sensor.RecordReading(6.0);

        // Act
        sensor.UpdateThreshold(6.0);

        // Assert
        Assert.IsTrue(sensor.IsAlertTriggered);
    }
}