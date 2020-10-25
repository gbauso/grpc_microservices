package weather.util.metrics

import java.lang.management.ManagementFactory
import java.lang.reflect.InvocationTargetException
import java.lang.reflect.Method
import java.util.*


class ServerMetricsCollector {
    private val memoryBean = ManagementFactory.getMemoryMXBean()
    private val osBean = ManagementFactory.getOperatingSystemMXBean()
    val megaByte = 1024.0 * 1024.0


    fun getCpuUsage() = osBean.systemLoadAverage
    fun getFreeMemory() = ((memoryBean.nonHeapMemoryUsage.committed / megaByte) - getUsedMemory())
    fun getUsedMemory() = memoryBean.nonHeapMemoryUsage.used / megaByte

}