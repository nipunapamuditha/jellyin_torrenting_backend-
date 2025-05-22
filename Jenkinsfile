pipeline {
    agent any
    
    environment {
        // Update these with your server details
        DEPLOY_SERVER = "10.10.10.80"
        DEPLOY_DIR = "/home/jenkins"
        // Update with your actual repository
        GIT_REPO = "https://github.com/nipunapamuditha/jellyin_torrenting_backend-.git"
        GIT_BRANCH = "master"
    }
    
    stages {
        stage('Checkout') {
            steps {
                // Clean workspace before building
                cleanWs()
                
                // Clone the repository
                git branch: "${GIT_BRANCH}", url: "${GIT_REPO}"
            }
        }
        
        stage('Deploy to Server') {
            steps {
                // Replace with your actual SSH credentials ID
                sshagent(['0ff14880-bcc6-4400-a835-a66a5a3cf0ba']) {
                    sh '''
                    # Create deployment directory on remote server if it doesn't exist
                    ssh -o StrictHostKeyChecking=jenkins@${DEPLOY_SERVER} "sudo mkdir -p ${DEPLOY_DIR}_temp && sudo chown \$(whoami):\$(whoami) ${DEPLOY_DIR}_temp"
                    
                    # Copy project files to server
                    rsync -avz -e "ssh -o StrictHostKeyChecking=no" --exclude ".git" ./ jenkins@${DEPLOY_SERVER}:${DEPLOY_DIR}_temp/
                    
                    # Build Docker image on remote server
                    ssh -o StrictHostKeyChecking=no jenkins@${DEPLOY_SERVER} "cd ${DEPLOY_DIR}_temp && docker build -t jellyfin-torrent:latest ."
                    
                    # Stop existing containers if they exist
                    ssh -o StrictHostKeyChecking=no jenkins@${DEPLOY_SERVER} "cd ${DEPLOY_DIR} && docker-compose down || true"
                    
                    # Replace old directory with new deployment
                    ssh -o StrictHostKeyChecking=no jenkins@${DEPLOY_SERVER} "sudo rm -rf ${DEPLOY_DIR} && sudo mv ${DEPLOY_DIR}_temp ${DEPLOY_DIR}"
                    
                    # Start containers with docker-compose
                    ssh -o StrictHostKeyChecking=no jenkins@${DEPLOY_SERVER} "cd ${DEPLOY_DIR} && docker-compose up -d"
                    
                    # Check running containers
                    ssh -o StrictHostKeyChecking=no jenkins@${DEPLOY_SERVER} "docker ps | grep jellyfin"
                    '''
                }
            }
        }
    }
    
    post {
        success {
            echo 'Jellyfin torrenting backend deployed successfully!'
        }
        failure {
            echo 'Deployment failed. Check logs for details.'
        }
    }
}