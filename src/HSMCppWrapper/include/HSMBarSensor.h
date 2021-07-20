#pragma once

namespace hsm_wrapper
{
	template<class T>
	class HSMBarSensorImpl;

	template<class T>
	class HSMWRAPPER_API HSMBarSensor
	{
	public:
		using type = T;

		HSMBarSensor(std::shared_ptr<HSMBarSensorImpl<T>> sensor_impl);
		HSMBarSensor(HSMBarSensor&& sensor);
		~HSMBarSensor() = default;
		HSMBarSensor() = delete;
		HSMBarSensor(const HSMBarSensor&) = delete;
		HSMBarSensor& operator=(const HSMBarSensor&) = delete;
		HSMBarSensor& operator=(HSMBarSensor&& sensor) = delete;

		void AddValue(T value);
	private:
		std::shared_ptr<HSMBarSensorImpl<T>> impl;
	};




	using IntBarSensor = HSMBarSensor<int>;
	using DoubleBarSensor = HSMBarSensor<double>;
}