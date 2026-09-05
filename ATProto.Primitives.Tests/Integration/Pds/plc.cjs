// Local creation/resolution test double, not a full PLC directory.
// Use the reference PLC library bundled in the pinned PDS image to validate
// genesis signatures/DIDs and format documents. No public directory is contacted.
const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');
const packages = '/app/node_modules/.pnpm';
const library = fs.readdirSync(packages).find(name => name.startsWith('@did-plc+lib@'));
const plc = require(path.join(packages, library, 'node_modules/@did-plc/lib'));
const documents = new Map();

http.createServer(async (req, res) => {
  res.setHeader('Content-Type', 'application/json');
  try {
    const did = decodeURIComponent(new URL(req.url, 'http://plc').pathname.slice(1));
    if (req.method === 'GET' && did === '_health') {
      return res.end(JSON.stringify({ status: 'ok' }));
    }
    if (!/^did:plc:[a-z2-7]{24}$/.test(did)) {
      res.writeHead(400);
      return res.end(JSON.stringify({ error: 'InvalidDid' }));
    }
    if (req.method === 'POST') {
      if (documents.has(did)) {
        res.writeHead(409);
        return res.end(JSON.stringify({ error: 'AlreadyExists' }));
      }
      let body = '';
      for await (const chunk of req) {
        body += chunk;
        if (body.length > 65536) throw new Error('Operation too large');
      }
      const data = await plc.assureValidCreationOp(did, JSON.parse(body));
      documents.set(did, plc.formatDidDoc(data));
      return res.end('{}');
    }
    if (req.method === 'GET' && documents.has(did)) {
      return res.end(JSON.stringify(documents.get(did)));
    }
    res.writeHead(404);
    res.end(JSON.stringify({ error: 'NotFound' }));
  } catch (error) {
    res.writeHead(400);
    res.end(JSON.stringify({ error: error.message }));
  }
}).listen(3000, '0.0.0.0');
