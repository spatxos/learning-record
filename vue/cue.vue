<template>
  <div>
    <h1>世界遗产</h1>
    <ul>
      <li v-for="heritage in heritages" :key="heritage.id">
        <a :href="heritage.url">
          <img :src="heritage.image" alt="世界遗产">
          <h2>{{ heritage.name }}</h2>
          <p>{{ heritage.country }}</p>
        </a>
      </li>
    </ul>
  </div>
</template>

<script>
export default {
  data() {
    return {
      heritages: [
        {
          id: 1,
          name: "大堡礁",
          country: "澳大利亚",
          image: "https://upload.wikimedia.org/wikipedia/commons/thumb/0/05/Great_Barrier_Reef_from_space.jpg/1200px-Great_Barrier_Reef_from_space.jpg",
          url: "https://en.wikipedia.org/wiki/Great_Barrier_Reef",
        },
        {
          id: 2,
          name: "万里长城",
          country: "中国",
          image: "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a3/The_Great_Wall_of_China_from_space.jpg/1200px-The_Great_Wall_of_China_from_space.jpg",
          url: "https://en.wikipedia.org/wiki/Great_Wall_of_China",
        },
        {
          id: 3,
          name: "玛雅遗址",
          country: "墨西哥",
          image: "https://upload.wikimedia.org/wikipedia/commons/thumb/0/03/Chichen_Itza_%28cropped%29.jpg/1200px-Chichen_Itza_%28cropped%29.jpg",
          url: "https://en.wikipedia.org/wiki/Chichen_Itza",
        },
      ],
    };
  },
};
</script>

<style>
body {
  font-family: "Avenir", Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}

h1 {
  font-size: 2em;
}

ul {
  list-style-type: none;
  margin: 0;
  padding: 0;
}

li {
  margin-bottom: 10px;
}

a {
  text-decoration: none;
  color: #2c3e50;
}

a:hover {
  text-decoration: underline;
}

img {
  width: 100%;
}
</style>