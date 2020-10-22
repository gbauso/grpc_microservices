package weather.util.metrics

import java.lang.management.ManagementFactory
import java.lang.reflect.InvocationTargetException
import java.lang.reflect.Method
import java.util.*


class ServerMetricsCollector {
    private val osBean = ManagementFactory.getOperatingSystemMXBean()

    fun getCpuUsage() = invokeDouble(getMethod("getProcessCpuLoad"))
    fun getFreeMemory() = invokeDouble(getMethod("getFreePhysicalMemorySize"))
    fun getUsedMemory() = invokeDouble(getMethod("getTotalPhysicalMemorySize")) - getFreeMemory()

    private fun getMethod(name: String): Optional<Method> {
        return try {
            val method: Method = osBean.javaClass.getDeclaredMethod(name).also {
                it.isAccessible = true
            }
            Optional.of(method)
        } catch (e: NoSuchMethodException) {
            Optional.empty()
        }
    }

    private fun invokeDouble(method: Optional<Method>): Double {
        return if (method.isPresent) {
            try {
                method.get().invoke(osBean) as Double
            } catch (ite: IllegalAccessException) {
                0.0
            } catch (ite: InvocationTargetException) {
                0.0
            }
        } else 0.0
    }
}