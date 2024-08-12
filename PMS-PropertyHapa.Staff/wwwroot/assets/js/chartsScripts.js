// ************ Cash Flow Chart*****************

var optionsCash = {
  series: [
    {
      name: "Income",
      data: ["4k", "6k", "5k", "7k", "6k", "8k", "7k"],
    },
    {
      name: "Expenses",
      data: ["3k", "5k", "2k", "8k", "5k", "7k", "8k"],
    },
  ],
  chart: {
    type: "bar",
    height: 500,
    toolbar: {
      show: false, // Hide the toolbar
    },
  },
  colors: ["#0162DD", "#FDB813"],
  plotOptions: {
    bar: {
      horizontal: false,
      columnWidth: "85%",
      endingShape: "rounded",
      borderRadius: 3,
    },
  },
  dataLabels: {
    enabled: false,
  },
  stroke: {
    show: true,
    width: 2,
    colors: ["transparent"],
  },
  xaxis: {
    categories: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul"],
  },
  fill: {
    opacity: 1,
  },
  tooltip: {
    custom: function ({ series, seriesIndex, dataPointIndex, w }) {
      const seriesName = w.config.series[seriesIndex].name;
      const tooltipClass =
        seriesName === "Expenses" ? "expense_div" : "income_div";

      return `
         <div class="${tooltipClass}">
        <p>
        ${w.globals.labels[dataPointIndex]} 2024
       <p>
       <h3>
       $ ${series[seriesIndex][dataPointIndex]} 
      <h3>
       <p>
        ${seriesName} 
       <p>
        </div>`;
    },
  },
};

var cashChart = new ApexCharts(
  document.querySelector("#cashFlowChart"),
  optionsCash
);
cashChart.render();

function setChartHeight() {
  if (window.innerWidth < 992) {
    cashChart.updateOptions({
      chart: {
        height: 400,
      },
    });
  } else {
    cashChart.updateOptions({
      chart: {
        height: 500,
      },
    });
  }
}

window.addEventListener("resize", setChartHeight);
setChartHeight(); // Initial call to set the chart height

// ************ Cash Flow Chart End*****************

// ************ Occupancy %age Chart*****************

var optionsOccupancy = {
  series: [14, 86],
  labels: ["Total Occupancy", "Occupancy Percentage"],
  chart: {
    type: "donut",
    width: 350,
  },
  colors: ["#0162DD", "#00DB2D"],
  plotOptions: {
    pie: {
      donut: {
        labels: {
          show: true,
          total: {
            show: true,
            label: "Occupancy",
            formatter: function (w) {
              return "86%";
            },
            fontSize: "12px",
          },
        },
      },
    },
  },
  dataLabels: {
    enabled: false,
  },
  legend: {
    position: "bottom",
    labels: {
      colors: "#939393",
    },
  },
};

var chartOccupancy = new ApexCharts(
  document.querySelector("#occupancyChart"),
  optionsOccupancy
);
chartOccupancy.render();

// ************ Occupancy %age Chart End*****************

// ************ Vacancy Chart*****************

var optionsVacancy = {
  series: [15, 85],
  labels: ["Total Vacancy", "Vacancy Percentage"],
  chart: {
    type: "donut",
    width: 350,
  },
  colors: ["#0162DD", "#FDB813"],
  plotOptions: {
    pie: {
      donut: {
        labels: {
          show: true,
          total: {
            show: true,
            label: "Vacancy",
            formatter: function (w) {
              return "85%";
            },
            fontSize: "12px",
          },
        },
      },
    },
  },
  dataLabels: {
    enabled: false,
  },
  legend: {
    position: "bottom",
    labels: {
      colors: "#939393",
    },
  },
};

var chartVacancy = new ApexCharts(
  document.querySelector("#vacancyChart"),
  optionsVacancy
);
chartVacancy.render();

// ************ Vacancy Chart End*****************

// ************ Rent Colection Chart*****************

var weeklyData = {
  series: [
    {
      name: "Unpaid",
      data: [
        { x: "Sun", y: 1500 },
        { x: "Mon", y: 1300 },
        { x: "Tue", y: 1400 },
        { x: "Wed", y: 1200 },
        { x: "Thu", y: 1300 },
        { x: "Fri", y: 1000 },
        { x: "Sat", y: 900 },
      ],
    },
    {
      name: "Paid",
      data: [
        { x: "Sun", y: 1300 },
        { x: "Mon", y: 800 },
        { x: "Tue", y: 1000 },
        { x: "Wed", y: 900 },
        { x: "Thu", y: 1500 },
        { x: "Fri", y: 1200 },
        { x: "Sat", y: 1400 },
      ],
    },
  ],
};

var monthlyData = {
  series: [
    {
      name: "Unpaid",
      data: [
        { x: "Week 1", y: 6500 },
        { x: "Week 2", y: 5800 },
        { x: "Week 3", y: 6200 },
        { x: "Week 4", y: 5900 },
      ],
    },
    {
      name: "Paid",
      data: [
        { x: "Week 1", y: 5500 },
        { x: "Week 2", y: 4800 },
        { x: "Week 3", y: 5100 },
        { x: "Week 4", y: 6200 },
      ],
    },
  ],
};

var optionsRent = {
  chart: {
    height: 370,
    type: "area",
    toolbar: {
      show: false,
    },
  },
  colors: ["#0095FF", "#00DB2D"],
  dataLabels: {
    enabled: false,
  },
  stroke: {
    curve: "smooth",
  },
  legend: {
    show: true,
    customLegendItems: ["Unpaid", "Paid"],
    inverseOrder: true,
    labels: {
      colors: "#939393",
    },
  },
};

// Initialize with weekly data
optionsRent.series = weeklyData.series;

var chartRent = new ApexCharts(
  document.querySelector("#rentCollectionChart"),
  optionsRent
);
chartRent.render();

// Event listener for weekly filter
document
  .getElementById("weeklyRentFilter")
  .addEventListener("click", function (e) {
    e.preventDefault();
    optionsRent.series = weeklyData.series;
    chartRent.updateOptions(optionsRent);
  });

// Event listener for monthly filter
document
  .getElementById("monthlyRentFilter")
  .addEventListener("click", function (e) {
    e.preventDefault();
    optionsRent.series = monthlyData.series;
    chartRent.updateOptions(optionsRent);
  });

// ************ Rent Colection Chart End*****************

// ************ Task Sumary Chart *****************
// Define sample data for sparkline charts for task summary chart
var sparklineData = [
  30, 40, 25, 50, 49, 21, 70, 51, 42, 30, 40, 25, 50, 49, 21, 70, 51, 42,
];

// Function to randomize array (if needed)
function randomizeArray(array) {
  return array.slice().sort(() => Math.random() - 0.5);
}

// Define chart options
var optionsTask = {
  series: [
    {
      data: randomizeArray(sparklineData),
    },
  ],
  chart: {
    type: "area",
    height: 160,
    sparkline: {
      enabled: true,
    },
  },
  stroke: {
    curve: "smooth",
  },
  fill: {
    opacity: 0.3,
  },
  xaxis: {
    crosshairs: {
      width: 1,
    },
  },
  yaxis: {
    min: 0,
  },
  colors: ["#4318FF"],
  //   title: {
  //     text: "$424,652",
  //     offsetX: 0,
  //     style: {
  //       fontSize: "24px",
  //     },
  //   },
  //   subtitle: {
  //     text: "Sales",
  //     offsetX: 0,
  //     style: {
  //       fontSize: "14px",
  //     },
  //   },
};

// Initialize chartTask as a global variable
var chartTask;

// Render charts using ApexCharts
document.addEventListener("DOMContentLoaded", function () {
  chartTask = new ApexCharts(
    document.querySelector("#taskSummaryChart"),
    optionsTask
  );
  chartTask.render();

  // Event listeners for dropdown items
  document
    .getElementById("monthlyFilter")
    .addEventListener("click", function (event) {
      event.preventDefault();
      updateChart("monthly");
    });

  document
    .getElementById("yearlyFilter")
    .addEventListener("click", function (event) {
      event.preventDefault();
      updateChart("yearly");
    });
});

// Function to update chart based on filter selection
function updateChart(filter) {
  var newData;

  if (filter === "monthly") {
    newData = randomizeArray(sparklineData.slice(0, 12)); // Use first 12 elements for monthly data
  } else if (filter === "yearly") {
    newData = randomizeArray(sparklineData); // Use all elements for yearly data
  }

  chartTask.updateSeries([{ data: newData }]);
}

// ************ Task Sumary Chart End*****************
