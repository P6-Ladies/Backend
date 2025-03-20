// .\src\index.js
require('dotenv').config({ path: '.env' });

const { spawn } = require('child_process');
const path = require('path');
const pythonScriptPath = path.join(__dirname, 'services', 'run_model.py');
 
function testHuggingFace(){
    return new Promise((resolve, reject) =>{
        const py = spawn('python3', [pythonScriptPath, process.env.MODEL_NAME]);

        let outputData = '';
        py.stdout.on('data', (data) =>{
            outputData += data.toString();
        });

        py.stderr.on('data', (err)=>{
            console.error('run_model.py error: ', err.toString());
        });

        py.on('close', (code)=>{
            if (code === 0){
                console.log('HuggingFace model called successfully.');
                console.log(`Output: \n`, outputData);
                resolve();
            } else{
                reject(new Error(`run_model.py closed with code: ${code}`));
            }
        });
    });
}

async function main() {
    console.log(`Environment: ${process.env.NODE_ENV}`);
    console.log(`HuggingFace Model: ${process.env.MODEL_NAME}`);


    try{
        await testHuggingFace()
    } catch (err){
        console.log(`HuggingFace test failed: `, err);
        process.exit(1);
    }
    console.log(`All checks passed successfully!`);
    process.exit(0);
};


main();
