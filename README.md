# House of Curry — Lunch Order App

A simple Angular web app for placing daily lunch orders at House of Curry.

## Features

- Save name, phone, and delivery location in localStorage
- One-tap Veg / Non-Veg combo ordering with included items shown
- Add extra items (sambar rice, curries, etc.)
- Sticky checkout bar with total
- Zelle payment info plus Apple Pay / Google Pay placeholder buttons
- Copy-ready order message for DM

## Local development

```bash
cd house-of-curry
npm install
npm start
```

Open [http://localhost:4200](http://localhost:4200).

## GitHub Pages

The app deploys automatically to GitHub Pages on push to `main`.

Live site: `https://<your-username>.github.io/Houseofcurry/`

### Manual deploy

```bash
cd house-of-curry
npm run build:gh-pages
npx angular-cli-ghpages --dir=dist/house-of-curry/browser
```

## Updating today's menu

Edit `src/app/services/menu.service.ts` with the daily items, prices, and delivery locations.

## Contact defaults

- Zelle: 571-722-2640
- Delivery: 901, 47300, 45500
- Email: Houseofcurrys.ca@gmail.com
