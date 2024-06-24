window.onload = main;
function main() {
  // 获取canvas元素
  var canvasEl = document.getElementById('canvas');
  var ctx = canvasEl.getContext('2d');
  // canvas画布的 背景颜色
  var backgroundColor = document.getElementsByClassName('background-color')

  // canvas画布的宽 等于 可视区域的宽
  canvasEl.width = canvasEl.clientWidth;
  // canvas画布的高 等于 可视区域的高
  canvasEl.height = canvasEl.clientHeight;

  // 保存小水珠的数组
  // 雨滴下落后散成小水珠，小水珠就是一些圆弧
  var dropList = [];

  // 重力
  // 雨滴下落后散成小水珠，小水珠会先上升后下降，主要是因为 gravity 这个变量的缘故
  var gravity = 0.5;

  // 保存雨滴的数组
  // 每个雨滴 都是 画的一条线 
  var linelist = [];

  // 保存鼠标的坐标 
  // mousePos[0] 代表x轴的值，mousePos[1] 代表y轴的值 
  var mousePos = [0, 0];

  // 跟随鼠标， mouseDis 大小区域内的雨滴会消失，形成散落效果
  // 以mousePos为圆心，mouseDis为半径，这个范围内的雨滴 都会散开，形成许多小水珠
  var mouseDis = 35;

  // 更新一次动画，画lineNum 条雨滴，lineNum 值越大，下雨就越密集
  var lineNum = 3;

  // 跟随鼠标方向 变化下雨方向的 速度
  // 鼠标移动后，下雨的方向 会慢慢改变，主要靠speedx 这个变量
  var speedx = 0;

  // maxspeedx 为 speedx 可以取的最大值
  // 当 speedx = maxspeedx时，下雨方向 会 随鼠标移动方向立即改变
  var maxspeedx = 0;

  // 页面大小发生变化时，重置canvas画布大小
  window.onresize = function () {
    canvasEl.width = canvasEl.clientWidth;
    canvasEl.height = canvasEl.clientHeight;
  }

  //移动鼠标触发事件
  window.onmousemove = function (e) {
    //  设置mousePos 等于 鼠标坐标
    //  e.clientX 为距离 浏览器窗口可视区域 左边的距离
    //  e.clientY 为距离 浏览器窗口可视区域 上边的距离
    mousePos[0] = e.clientX;
    mousePos[1] = e.clientY;

    // 通过鼠标位置，设置 maxspeedx的值，取值范围是 -1 到 1
    // maxspeedx的值，关系到 
    // 1、雨滴的方向
    // 2、雨滴下落的方向
    // 3、雨滴下落方向 跟随 鼠标移动方向变化的速度
    // 4、小水珠的移动方向
    // 值越接近1，表示方向越向右
    // 值越接近-1，表示方向越向左
    maxspeedx = (e.clientX - canvasEl.clientWidth / 2) / (canvasEl.clientWidth / 2);
  }

  // 根据参数，返回一个rgb颜色，用于给雨滴设置颜色
  function getRgb(r, g, b) {
    return "rgb(" + r + "," + g + "," + b + ")";
  }

  // 画 一滴雨（一条线）
  function createLine(e) {
    // 随机生成 雨滴的长度
    var temp = 0.2 * (50 + Math.random() * 100);
    // 一个 line 对象，代表一个雨滴
    var line = {
      // 雨滴下落速度  
      speed: 5.5 * (Math.random() * 6 + 3),
      // 判断是否删除，值为true就删除
      die: false,
      // 雨滴x坐标 
      posx: e,
      // 雨滴y坐标 
      posy: -50,
      // 雨滴的长度
      h: temp,
      // 雨滴的颜色
      color: getRgb(Math.floor(temp * 255 / 75), Math.floor(temp * 255 / 75), Math.floor(temp * 255 / 75))
    };
    // 把创建好的line（雨滴）对象，添加到保存雨滴的数组
    linelist.push(line);
  }

  // 画一个小水珠（雨滴散开后的小水珠就是一个个的圆弧）
  function createDrop(x, y) {
    // 一个 drop 对象，代表一个圆弧
    var drop = {
      // 判断是否删除，值为true就删除
      die: false,
      // 圆弧圆心的x坐标 
      posx: x,
      // 圆弧圆心的y坐标 
      posy: y,
      // vx 表示 x轴的值 变化的速度
      vx: (Math.random() - 0.5) * 8,
      // vy 表示 y轴的值 变化的速度 取值范围：-3 到 -9
      vy: Math.random() * (-6) - 3,
      // 圆弧的半径
      radius: Math.random() * 1.5 + 1
    };
    return drop;
  }

  // 画一定数量的小水珠
  function madedrops(x, y) {
    // 随机生成一个数 maxi
    // maxi 代表要画小水珠的数量
    var maxi = Math.floor(Math.random() * 5 + 5);
    for (var i = 0; i < maxi; i++) {
      dropList.push(createDrop(x, y));
    }
  }

  // 开始调用update函数，更新动画
  window.requestAnimationFrame(update);
  // 更新动画
  function update() {
    // 如果保存小水珠的数组有内容
    if (dropList.length > 0) {
      // 遍历保存小水珠的数组
      dropList.forEach(function (e) {
        //设置e.vx，vx表示x坐标变化的速度
        // (speedx)/2 是为了，让小水珠 在x轴的移动距离短一点，看上去更真实点
        // 也使 小水珠的移动方向 和 雨滴方向，雨滴下落方向，鼠标移动方向相同
        e.vx = e.vx + (speedx / 2);
        e.posx = e.posx + e.vx;

        //设置e.vy，vy表示y坐标变化的速度
        // e.vy的范围是-3 到 -9，而这时e.posy（y坐标）一定是正值，所以 e.posy的值会先减小后增大
        // 也就是实现 雨滴散成小水珠，小水珠会先上升后下降的效果
        e.vy = e.vy + gravity;
        e.posy = e.posy + e.vy;

        // 如果 小水珠y坐标 大于 可视区域的高度，设置die属性为true
        // 小水珠如果超出可视区域就删除掉
        if (e.posy > canvasEl.clientHeight) {
          e.die = true;
        }
      });
    }

    // 删除 die属性为ture 的数组成员
    // 可视区域外的小水珠删除掉
    for (var i = dropList.length - 1; i >= 0; i--) {
      if (dropList[i].die) {
        dropList.splice(i, 1);
      }
    }

    // 设置下雨方向变换的速度，取值范围： -1 到 1
    // 当 speedx = maxspeedx时，下雨方向 会 随鼠标移动方向立即改变
    speedx = speedx + (maxspeedx - speedx) / 50;

    // 根据lineNum的值，画一定数量雨滴
    for (var i = 0; i < lineNum; i++) {
      // 调用createLine 函数，参数是雨滴x坐标
      createLine(Math.random() * 2 * canvasEl.width - (0.5 * canvasEl.width));
    }

    // 设置结束线，也就是雨滴散开 形成许多小水珠的位置
    var endLine = canvasEl.clientHeight - Math.random() * canvasEl.clientHeight / 5;

    // 遍历保存雨滴的数组
    linelist.forEach(function (e) {

      // 利用勾股定理 确定一个范围，在这个范围内雨滴会散开形成小水珠
      // e.posx + speedx * e.h 是雨滴x坐标
      // e.posy + e.h 是雨滴y坐标
      var dis = Math.sqrt(((e.posx + speedx * e.h) - mousePos[0]) * ((e.posx + speedx * e.h) - mousePos[0]) + (e.posy + e.h - mousePos[1]) * (e.posy + e.h - mousePos[1]));

      // 如果在mouseDis区域内，就删除雨滴，画一些小水珠（圆弧）
      // 实现鼠标碰到雨滴，雨滴散成小水珠的效果
      if (dis < mouseDis) {
        // 删除 雨滴
        e.die = true;
        // 画一些小水珠（圆弧）
        madedrops(e.posx + speedx * e.h, e.posy + e.h);
      }

      // 如果雨滴超过 结束线，删除雨滴，画一些小水珠（圆弧）
      if ((e.posy + e.h) > endLine) {
        e.die = true;
        madedrops(e.posx + speedx * e.h, e.posy + e.h);
      }

      // 如果 雨滴 y坐标 大于 可视区域的高度，设置die属性为true
      // 如果 雨滴 超出可视区域就删除掉
      if (e.posy >= canvasEl.clientHeight) {
        e.die = true;
      } else {
        // 逐渐增加 雨滴 y坐标的值
        e.posy = e.posy + e.speed;

        // 变化雨滴 x坐标
        // * speedx 用来控制雨滴 下落 方向
        // 使 雨滴下落方向 和 鼠标移动方向相同
        e.posx = e.posx + e.speed * speedx;
      }
    });

    // 删除 die属性为ture 的数组成员
    // 鼠标区域内的，超过结束线的，可视区域外的雨滴删除掉
    for (var i = linelist.length - 1; i >= 0; i--) {
      if (linelist[i].die) {
        linelist.splice(i, 1);
      }
    }

    // 渲染
    render();
    // 递归调用 update，实现动画效果
    window.requestAnimationFrame(update);
  }

  // 渲染
  function render() {
    // 画一个和可视区域一样大的矩形
    ctx.fillStyle = backgroundColor;
    ctx.fillRect(0, 0, canvasEl.width, canvasEl.height);

    // 画雨滴效果
    ctx.lineWidth = 5;
    linelist.forEach(function (line) {
      ctx.strokeStyle = line.color;
      ctx.beginPath();
      ctx.moveTo(line.posx, line.posy);

      // * speedx 用来控制雨滴方向
      // 使 雨滴方向 和 鼠标移动方向相同
      ctx.lineTo(line.posx + line.h * speedx, line.posy + line.h);
      ctx.stroke();
    });

    // 画雨滴散开形成小水珠效果
    ctx.lineWidth = 1;
    ctx.strokeStyle = "#fff";
    dropList.forEach(function (e) {
      ctx.beginPath();
      ctx.arc(e.posx, e.posy, e.radius, Math.random() * Math.PI * 2, 1 * Math.PI);
      ctx.stroke();
    });

    // 解开注释，可看见鼠标范围
    /*
      ctx.beginPath();
      ctx.arc(mousePos[0], mousePos[1], mouseDis, 0, 2 * Math.PI);
      ctx.stroke();
    */



  }
}
var svg = d3.select("svg");
const g = svg.append("g");

var projectmethod = d3.geoMercator().center([120.982, 24.15]).scale(8000);
var pathGenerator = d3.geoPath().projection(projectmethod);
d3.json("/_img/json/COUNTY_MOI_1090820.json")
  .then(data => {
    const geometries = topojson.feature(data, data.objects["COUNTY_MOI_1090820"])

    g.append("path")
    const paths = g.selectAll("path").data(geometries.features);
    paths.enter()
      .append("path")
      .attr("d", pathGenerator)
      .attr("class", "county")
      // 加上簡易版本 tooltip
      .append("title")
      .text(d => d.properties["COUNTYNAME"])
  })
let weather = [], city = [], minTemp = [], maxTemp = [];
// var x = document.getElementById("A63000")
// console.log(x)

function getWeatherIcon(weatherCode) {
  // 将天气代码转换为两位数的字符串形式
  weatherCode = `${weatherCode}`.padStart(2, "0");
  // 返回天气图标的 URL，天气图标通常存储在特定路径下，路径中包含天气代码
  return `https://www.cwa.gov.tw/V8/assets/img/weather_icons/weathers/svg_icon/day/${weatherCode}.svg`;
}

function setInfomation(cityName) {
  let index = city.indexOf(cityName);

  $('#country').text(cityName);
  $('#cityTd').text(cityName);
  $('#cityTd1').text(cityName);

  let targetWeather = weather[index];// 从数组中获取特定索引位置的天气数据

  let targetMaxTemp = maxTemp[index];
  // console.log(maxTemp);
  let targetMinTemp = minTemp[index];

  // 遍历该天气数据中的时间信息
  $(targetWeather).find("time").each(function (index, elm) {       // .each(function (elm))只有找到 time的所有tag元素

    // let timeElm =$(".time")[index];   //<span class="time"></span>
    console.log(targetWeather);
    let timeElm = $($(".time")[index]);
    // console.log(timeElm);//取得.time 索引個數
    let weatherIconElm = $($(".weather-icon")[index]);
    // console.log(weatherIconElm); //取得.weather-icon 個術數輛 兩個
    let datelineElm = $($(".dateline")[index]);
    // console.log(datelineElm)
    // --------------分隔線-------------------------
    // 获取开始时间和结束时间，并提取小时和分钟信息
    // 从天气数据中找到开始时间元素，并获取其文本内容   

    // 获取 popular1 到 popular5 元素的文本内容

    $(elm).find("popular").each(function (popIndex, popElm) {
      // 获取热门景点的文本内容
      let popular = $(popElm).text();
      // 使用类选择器来选择相应的<span>元素，并填充文本内容
      $('.top').eq(popIndex).text("TOP" + (popIndex + 1) + ": " + popular);
    });

    let startTime = $(elm).find("startTime").text();
    let month = startTime.substring(6, 7);
    let date = startTime.substring(8, 10);

    // 从开始时间中提取小时部分，并将其转换为整数
    let startHour = parseInt(startTime.substring(11, 13));
    // 从开始时间中提取分钟部分，并将其转换为整数
    let startMinute = parseInt(startTime.substring(14, 16));
    // 从天气数据中找到结束时间元素，并获取其文本内容
    let endTime = $(elm).find("endTime").text();
    // 从结束时间中提取小时部分，并将其转换为整数
    let endHour = parseInt(endTime.substring(11, 13));
    // 从结束时间中提取分钟部分，并将其转换为整数
    let endMinute = parseInt(endTime.substring(14, 16));
    // --------------分隔線-------------------------
    // $(targetTemp).find("time").each(function (index, elm) {
    //     let heightElm = $($(".height")[index]);
    //     let maxT = $(elm).find("parameter").each(index, elm).text();
    //     console.log(maxT)
    $(targetMaxTemp).find("time").each(function (index, elm) {
      // console.log(targetMaxTemp);//ok
      let heightElm = $($(".height")[index]);
      // console.log(heightElm)//ok
      let maxT = $(elm).find("parameter").text();
      $(".height").eq(index).text(maxT);

    })
    $(targetMinTemp).find("time").each(function (index, elm) {
      let lowElm = $($(".low")[index]);
      let minT = $(elm).find("parameter").text();
      $(".low").eq(index).text(minT);
    })
    // let tempStr=`${minT}-${maxT}`;

    // 判斷是否在白天時間範圍內
    // if ((startHour >= 6 && startHour < 18) || (startHour >= 18 && startHour < 24)) {
    // console.log("白天");
    // } else {
    // console.log("晚天");
    // };
    // 更新时间元素的文本内容，格式为：开始时间 - 结束时间
    // timeElm.text(`${startHour.toString().padStart(2, "0")}:${startMinute.toString().padStart(2, "0")} - ${endHour.toString().padStart(2, "0")}:${endMinute.toString().padStart(2, "0")}`)
    // 更新天气图标元素的 HTML 内容，使用相应的天气图标
    weatherIconElm.html(`<img src='${getWeatherIcon($(elm).find("parameterValue").text())}'>`)
    // 输出调试信息，显示时间、开始时间、小时和分钟 
    console.log($(elm).text(), "\n", startTime, "\n", startHour, "\n", startMinute);
    datelineElm.text(`${month}月${date}日`);
    // $(".temp").eq(index).text(tempStr);

  })
  $('.start').text('°' + '_').show();
  $('.end').text('°').show();
}


$(function () {
    setTimeout(
        () => {

            $("path").on("click", (event) => {

                let index = city.indexOf($(event.target).text());
                console.log(index);

                let targetCity = city[index];
                console.log(targetCity);
                setInfomation(targetCity);
                getFile(targetCity);
            });
            setInfomation("臺北市");
        },
        1000
    )

    function getFile(targetCity) {
        var xhr = new XMLHttpRequest();
        xhr.onload = function () {
            var data = xhr.responseText;
            var temp = JSON.parse(data);
            var yiLanArray = temp[targetCity];
            console.log(yiLanArray);
            const extractContent = (path) => {
                const match = path.match(/\/([^\/]+)\.jpg$/);
                return match ? match[1] : null;
            };
            const extractedContents = yiLanArray.map(extractContent).filter(content => content !== null);

            $('#divResult').html(temp);

            const range = (start, stop, step) => Array.from({ length: Math.ceil((stop - start) / step) }, (_, i) => start + (i * step));

            let innerHtml = ``;
            let indicatorsHtml = ``;
            for (let i of range(0, yiLanArray.length, 3)) {
                innerHtml += `<div class="carousel-item ${i === 0 ? "active" : ""}">
                  <div class="row">
                      <div class="p-2 col-4 md h-100 position-relative">
                          <img src="${yiLanArray[i]}" class="d-block w-33" alt="${extractedContents[i]}">                  
                          <p style="background-color: aliceblue;">${extractedContents[i]}</p>
                      </div>
                      ${i + 1 < yiLanArray.length ? `
                      <div class="p-2 col-4 md h-100 position-relative">
                          <img src="${yiLanArray[i + 1]}" class="d-block w-33" alt="${extractedContents[i + 1]}">          
                          <p style="background-color: aliceblue;">${extractedContents[i + 1]}</p>
                      </div>` : ""}
                      ${i + 2 < yiLanArray.length ? `
                      <div class="p-2 col-4 md h-100 position-relative">
                          <img src="${yiLanArray[i + 2]}" class="d-block w-33" alt="${extractedContents[i + 2]}">         
                          <p style="background-color: aliceblue;">${extractedContents[i + 2]}</p>
                      </div>` : ""}
                  </div>
              </div>`;
                indicatorsHtml += `<button type="button" data-bs-target="#carouselExampleCaptions" data-bs-slide-to="${i / 3}" ${i === 0 ? 'class="active" aria-current="true"' : ''} aria-label="Slide ${i / 3 + 1}"></button>`;
            }
            $(".carousel-inner").html(innerHtml);
            $(".carousel-indicators").html(indicatorsHtml);
        }

        xhr.open('GET', '/景點圖片/paths.json');
        xhr.send();
    }
 


    // 網頁與伺服器進行通訊，
    // 取得資料或更新網頁內容，而不需要重新載入整個頁面
    var xhr = new XMLHttpRequest();

    // 當XMLHttpRequest物件完成載入時所要執行的函式。
    xhr.onload = function () {
        // console.log(xhr.responseText);


        var parser = new DOMParser();
        // 宣告一個變數將xml轉乘string存到變數
        var weatherXML = parser.parseFromString(xhr.responseText, "application/xml");
        // console.log(weatherXML);
        // 宣告一個變數利用jq.find 標籤名將所有的索引跟元素找出存到city並將元素印出


        var locations = $(weatherXML).find("locationName").each(function (index, elm) {
            city.push($(elm).text())

        })

        $(weatherXML).find("weatherElement").each(function (index, weatherElement) {
            if ($(weatherElement).find("elementName").text() == "Wx") {
                weather.push(weatherElement)
            }
        })
        $(weatherXML).find("weatherElement").each(function (index, weatherElement) {
            if ($(weatherElement).find("elementName").text() == "MaxT") {
                maxTemp.push(weatherElement)
            }// console.log(maxTemp);//ok
        })

        $(weatherXML).find("weatherElement").each(function (index, weatherElement) {
            if ($(weatherElement).find("elementName").text() == "MinT") {
                minTemp.push(weatherElement)
                // console.log(minTemp);//ok
            }
        })
    }
    xhr.open('GET', '/_img/xml/F-C0032-0051.xml');
    xhr.send();
    $(".weatherinfo").show();
}



(document).addEventListener('DOMContentLoaded', function () {
    const heartButtons = document.getElementsByClassName('heart');
    const noteContent = document.getElementById('note-content');
    Array.from(heartButtons).forEach(button => {
        button.addEventListener('click', function (event) {
            event.preventDefault(); // 阻止按钮的默认行为
            event.stopPropagation(); // 阻止事件冒泡

            const descriptionElement = button.nextElementSibling; // 获取按钮后的描述元素
            const descriptionText = descriptionElement.textContent.trim(); // 获取描述文本
            const listItem = document.createElement('p'); // 创建一个段落元素来存放描述文本
            listItem.textContent = descriptionText; // 设置段落的文本内容



            if (button.classList.contains('filled')) {
                button.classList.remove('filled');
                button.innerHTML = '&#9825;'; // 空心心形


                // 从note区域中移除对应的描述
                const noteItems = noteContent.getElementsByTagName('p');
                for (let item of noteItems) {
                    if (item.textContent === descriptionText) {
                        noteContent.removeChild(item);
                        break;

                    }
                }
            } else {
                button.classList.add('filled');
                button.innerHTML = '&#9829;'; // 实心心形

                // 将描述文本添加到note区域
                noteContent.appendChild(listItem);

            }
        });
    });



})
);
